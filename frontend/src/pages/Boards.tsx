import { useKeycloak } from '@react-keycloak/web';
import { useEffect, useState } from 'react';
import axios from 'axios';
import { useNavigate } from 'react-router-dom';

interface Board
{
  id: string;
  title: string;
  description?: string;
  wallpaperImageId?: string;
}

function Boards()
{
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const [boards, setBoards] = useState<Board[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() =>
  {
    if (initialized && keycloak.authenticated && keycloak.token)
    {
      const fetchBoards = async () =>
      {
        try
        {
          const response = await axios.get('/api/boards', {
            headers: { Authorization: `Bearer ${keycloak.token}` },
          });
          setBoards(response.data);
          setError(null);
        } catch (error)
        {
          console.error('Failed to fetch boards:', error);
          setError('Failed to load boards. Please try again later.');
        }
      };
      fetchBoards();
    } else if (initialized && !keycloak.authenticated)
    {
      navigate('/'); // Redirect to home if not authenticated
    }
  }, [initialized, keycloak, navigate]);

  return (
    <div className="min-h-screen bg-gray-100 text-gray-900 font-sans">

      <main className="container mx-auto p-6">
        <h2 className="text-3xl font-bold text-blue-800 mb-6">
          Your Boards
        </h2>
        {error && <p className="text-red-600 mb-4">{error}</p>}
        {boards.length > 0 ? (
          <ul className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
            {boards.map((board) => (
              <li
                key={board.id}
                className="bg-white p-4 rounded-lg shadow hover:shadow-lg transition"
              >
                <button
                  onClick={() => navigate(`/board/${board.id}`)}
                  className="text-blue-600 hover:underline text-xl font-medium"
                >
                  {board.title}
                </button>
                {board.description && (
                  <p className="text-sm text-gray-600 mt-1">{board.description}</p>
                )}
              </li>
            ))}
          </ul>
        ) : (
          <p className="text-gray-600">No boards yet. Create one to get started!</p>
        )}
      </main>

    </div>
  );
}

export default Boards;