﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager instance { get { return _session; } }

        int _sessionId = 0; // 0 부터 시작해서 쭉 상승
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        public ClientSession Generate()
        {
            lock(_lock)
            {
                int sessionId = ++_sessionId;

                ClientSession session = new ClientSession(); // 풀링하고 싶다면, queue 를 사용해도됨 
                session.SessionId = sessionId;
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Connected : {sessionId}");

                return session;
            }
        }

        public ClientSession Find(int id)
        {
            lock(_lock)
            {
                ClientSession session = null;
                _sessions.TryGetValue(id, out session);
                return session;
            }
        }

        public void Remove(ClientSession session)
        {
            lock(_lock)
            {
                _sessions.Remove(session.SessionId);
            }
        }
    
    }
}