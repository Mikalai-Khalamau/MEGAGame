
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MEGAGame.Core.Data;
using MEGAGame.Core.Models;
using Microsoft.EntityFrameworkCore;

public class GameHub : Hub
{
    private readonly GameDbContext _context;

    public GameHub(GameDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task CreateRoom(string playerName, int packId, bool isPublic)
    {
        try
        {
            Console.WriteLine($"CreateRoom called. PlayerName: {playerName}, PackId: {packId}, IsPublic: {isPublic}");

            if (string.IsNullOrEmpty(playerName) || packId <= 0)
            {
                throw new ArgumentException("Invalid player name or pack ID.");
            }

            var packExists = await _context.QuestionPacks.AnyAsync(p => p.PackId == packId);
            if (!packExists)
            {
                throw new ArgumentException($"Pack with ID {packId} does not exist.");
            }

            var sessionId = Guid.NewGuid().ToString();
            var joinKey = isPublic ? null : Guid.NewGuid().ToString();
            var session = new GameSession
            {
                SessionId = sessionId,
                HostId = Context.ConnectionId, // Используем ConnectionId вместо playerName
                StartTime = DateTime.UtcNow,
                Status = "waiting",
                PackId = packId,
                IsPublic = isPublic,
                JoinKey = joinKey ?? ""
            };

            Console.WriteLine($"Adding session to context: {sessionId}");
            _context.GameSessions.Add(session);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Session saved: {sessionId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await Clients.Caller.SendAsync("ReceiveSessionInfo", new { SessionId = sessionId, JoinKey = joinKey });
            Console.WriteLine($"Session info sent to client: {sessionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateRoom: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task JoinRoom(string sessionId, string playerName, string joinKey = null)
    {
        try
        {
            Console.WriteLine($"JoinRoom called. SessionId: {sessionId}, PlayerName: {playerName}, JoinKey: {joinKey}");
            var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null || (!session.IsPublic && session.JoinKey != joinKey))
            {
                Console.WriteLine("Session not found or invalid join key.");
                await Clients.Caller.SendAsync("JoinRoomFailed", "Session not found or invalid join key.");
                return;
            }

            var playerCount = await _context.SessionPlayers.CountAsync(sp => sp.SessionId == sessionId);
            if (playerCount >= 4)
            {
                Console.WriteLine("Room is full (max 4 players).");
                await Clients.Caller.SendAsync("JoinRoomFailed", "Room is full (max 4 players).");
                return;
            }

            var player = new SessionPlayer
            {
                SessionId = sessionId,
                PlayerId = Context.ConnectionId,
                Score = 0,
                LastUpdated = DateTime.UtcNow
            };
            _context.SessionPlayers.Add(player);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Player added to session: {sessionId}, PlayerId: {player.PlayerId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            var players = await _context.SessionPlayers.Where(sp => sp.SessionId == sessionId).ToListAsync();
            Console.WriteLine($"Sending UpdatePlayers to group: {sessionId}, Players: {players.Count}");
            await Clients.Group(sessionId).SendAsync("UpdatePlayers", players);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in JoinRoom: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task StartGame(string sessionId)
    {
        try
        {
            Console.WriteLine($"StartGame called. SessionId: {sessionId}");
            var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId && s.HostId == Context.ConnectionId);
            if (session == null || session.Status != "waiting")
            {
                Console.WriteLine("Session not found or not in waiting state.");
                await Clients.Caller.SendAsync("StartGameFailed", "Session not found or not in waiting state.");
                return;
            }

            var playerCount = await _context.SessionPlayers.CountAsync(sp => sp.SessionId == sessionId);
            if (playerCount < 2 || playerCount > 4)
            {
                Console.WriteLine($"Invalid player count: {playerCount} (must be 2-4).");
                await Clients.Caller.SendAsync("StartGameFailed", $"Invalid player count: {playerCount} (must be 2-4).");
                return;
            }

            session.Status = "playing";
            session.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine($"Session status updated to playing: {sessionId}");

            await Clients.Group(sessionId).SendAsync("GameStarted", sessionId);
            Console.WriteLine($"Game started for session: {sessionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in StartGame: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task SelectQuestion(string sessionId, int questionId)
    {
        try
        {
            Console.WriteLine($"SelectQuestion called. SessionId: {sessionId}, QuestionId: {questionId}");
            var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null || session.Status != "playing")
            {
                Console.WriteLine("Session not found or not in playing state.");
                return;
            }

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.QuestionId == questionId && q.PackId == session.PackId);
            if (question == null || question.IsPlayed)
            {
                Console.WriteLine("Question not found or already played.");
                return;
            }

            question.IsPlayed = true;
            session.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            Console.WriteLine($"Question marked as played: {questionId}");

            await Clients.Group(sessionId).SendAsync("UpdateQuestion", question, GetInitialTime(session));
            Console.WriteLine($"Question updated for session: {sessionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SelectQuestion: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task Buzz(string sessionId)
    {
        try
        {
            Console.WriteLine($"Buzz called. SessionId: {sessionId}");
            var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null || session.Status != "playing")
            {
                Console.WriteLine("Session not found or not in playing state.");
                return;
            }

            var player = await _context.SessionPlayers.FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == Context.ConnectionId);
            if (player == null)
            {
                Console.WriteLine("Player not found in session.");
                return;
            }

            await Clients.Group(sessionId).SendAsync("PlayerBuzzed", player.PlayerId);
            Console.WriteLine($"Player buzzed: {player.PlayerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in Buzz: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    public async Task SubmitAnswer(string sessionId, string answer)
    {
        try
        {
            Console.WriteLine($"SubmitAnswer called. SessionId: {sessionId}, Answer: {answer}");
            var session = await _context.GameSessions.FirstOrDefaultAsync(s => s.SessionId == sessionId);
            if (session == null || session.Status != "playing")
            {
                Console.WriteLine("Session not found or not in playing state.");
                return;
            }

            var player = await _context.SessionPlayers.FirstOrDefaultAsync(sp => sp.SessionId == sessionId && sp.PlayerId == Context.ConnectionId);
            if (player == null)
            {
                Console.WriteLine("Player not found in session.");
                return;
            }

            var question = await _context.Questions.FirstOrDefaultAsync(q => q.PackId == session.PackId && q.IsPlayed && q.Round == GetCurrentRound(session));
            if (question == null)
            {
                Console.WriteLine("No active question found.");
                return;
            }

            bool isCorrect = CheckAnswer(question, answer);
            if (isCorrect)
            {
                player.Score += question.Points;
            }
            await _context.SaveChangesAsync();
            Console.WriteLine($"Answer submitted. Player: {player.PlayerId}, Correct: {isCorrect}, Score: {player.Score}");

            await Clients.Group(sessionId).SendAsync("AnswerSubmitted", player.PlayerId, answer, isCorrect, player.Score);
            Console.WriteLine($"Answer result sent to group: {sessionId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in SubmitAnswer: {ex.Message}\nStackTrace: {ex.StackTrace}");
            throw;
        }
    }

    private bool CheckAnswer(Question question, string answer)
    {
        try
        {
            return question.Round == 1
                ? int.Parse(answer) == question.CorrectOption
                : answer.Trim().Equals(question.Answer?.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CheckAnswer: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return false;
        }
    }

    private int GetInitialTime(GameSession session)
    {
        var round = GetCurrentRound(session);
        return round switch { 1 => 10, 2 => 20, 3 => 30, 4 => 60, _ => 10 };
    }

    private int GetCurrentRound(GameSession session)
    {
        return _context.Questions
            .Where(q => q.PackId == session.PackId && q.IsPlayed)
            .Select(q => q.Round)
            .DefaultIfEmpty(1)
            .Max();
    }
}