using System;
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

        int _sessionId = 0; // session이 0 부터 상승하도록 구현 
        Dictionary<int, ClientSession> _sessions = new Dictionary<int, ClientSession>();
        object _lock = new object();

        /// <summary>
        /// 세션을 새로 생성할 때 사용. ClientSession 타입으로 session을 뱉어준다. 
        /// </summary>
        /// <returns></returns>
        public ClientSession Generate()
        {
            lock(_lock)
            {
                int sessionId = ++_sessionId; // 나는 ++을 보통 뒤에 많이 붙이는데 이렇게 앞에도 유용하게 사용가능

                ClientSession session = new ClientSession(); // 풀링하고 싶다면, queue 를 사용해도되지만, 굳이 풀링할 정도는 아니다.
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
