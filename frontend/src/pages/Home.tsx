import { useKeycloak } from '@react-keycloak/web';
import axios from 'axios';
import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

interface Board {
  id: string;
  title: string;
  description?: string;
  wallpaperImageId?: string;
}

function Home() {
  const { keycloak, initialized } = useKeycloak();
  const [boards, setBoards] = useState<Board[]>([]);

  useEffect(() => {
    if (initialized && keycloak.authenticated)
    {
      if (keycloak.token)
      {
        console.log('JWT header', JSON.parse(atob(keycloak.token.split('.')[0])));
        console.log('JWT:', keycloak.token);
      }
      const fetchBoards = async () => {
        try
        {
          const requestUrl = '/api/boards';
          const requestHeaders = { Authorization: `Bearer ${keycloak.token}` };
          console.log('Requesting boards:', { url: requestUrl, headers: requestHeaders });
          const response = await axios.get(requestUrl, {
            headers: requestHeaders,
          });
          setBoards(response.data);
        } catch (error) {
          console.error('Failed to fetch boards:', error);
        }
      };
      fetchBoards();
    }
  }, [initialized, keycloak]);

  return (
    <div>
      <h1>Boardis Kanban App</h1>
      {initialized ? (
        keycloak.authenticated ? (
          <>
            <p>Welcome, {keycloak.tokenParsed?.preferred_username || 'User'}</p>
            <button onClick={() => keycloak.logout({ redirectUri: window.location.origin + '/' })}>
              Logout
            </button>
            <h2>Your Boards</h2>
            <ul>
              {boards.map((board) => (
                <li key={board.id}>
                  <Link to={`/board/${board.id}`}>{board.title}</Link>
                  {board.description && (
                    <div style={{ fontSize: '0.95em', color: '#666', marginLeft: '1em' }}>
                      {board.description}
                    </div>
                  )}
                </li>
              ))}
            </ul>
          </>
        ) : (
          <>
            <p>Please log in to view your Kanban boards.</p>
            <button onClick={() => keycloak.login({ redirectUri: window.location.origin + '/' })}>
              Login
              </button>
              <button
              style={{ marginLeft: '1em' }}
              onClick={() => keycloak.register({ redirectUri: window.location.origin + '/' })}
            >
              Register
            </button>
          </>
        )
      ) : (
        <p>Loading...</p>
      )}
    </div>
  );
}

export default Home;