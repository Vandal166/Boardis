import { useKeycloak } from '@react-keycloak/web';
import { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import CreateBoardDropdown from '../components/CreateBoardDropdown';
import Spinner from '../components/Spinner';
import api from '../api';
import BoardSettingsPanel from '../components/BoardSettingsPanel';

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
  const [, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [search, setSearch] = useState('');
  const [fieldErrors, setFieldErrors] = useState<{ [key: string]: string[] }>({});

  const [showModal, setShowModal] = useState(false);
  const [createError, setCreateError] = useState<string | null>(null);
  const [creating, setCreating] = useState(false);
  // Track which board's settings panel is open
  const [openSettingsBoardId, setOpenSettingsBoardId] = useState<string | null>(null);
  const [settingsPanelPosition, setSettingsPanelPosition] = useState<{ top: number; right: number } | null>(null);
  const ellipsisRefs = useRef<{ [key: string]: HTMLButtonElement | null }>({});

  useEffect(() =>
  {
    if (initialized && keycloak.authenticated && keycloak.token)
    {
      setIsLoading(true);
      const fetchBoards = async () =>
      {
        try
        {
          const response = await api.get('/api/boards');
          setBoards(response.data);
          setError(null);
        } catch (error)
        {
          console.error('Failed to fetch boards:', error);
          setError('Failed to load boards. Please try again later.');
        }
        finally
        {
          setIsLoading(false);
        }
      };
      fetchBoards();

    }
    else if (initialized && !keycloak.authenticated)
    {
      navigate('/'); // Redirect to home if not authenticated
    }
  }, [initialized, keycloak, navigate]);

  // filtering boards by title and then description(if not null)
  const filteredBoards = boards.filter(board =>
    board.title.toLowerCase().includes(search.toLowerCase()) ||
    (board.description && board.description.toLowerCase().includes(search.toLowerCase()))
  );


  // Create board handler
  const handleCreateBoard = async (data: { title: string; description?: string; wallpaperImageId?: string }) =>
  {
    setCreateError(null);

    setCreating(true);
    try
    {
      const payload: any = { title: data.title };
      if (data.description) payload.description = data.description;
      if (data.wallpaperImageId) payload.wallpaperImageId = data.wallpaperImageId;

      const response = await api.post('/api/boards', payload);

      // Success: redirect to the new board
      if (response.data?.id)
      {
        setBoards(prev => [...prev, response.data]);
        setShowModal(false);
        navigate(`/dashboard/board/${response.data.id}`);
      } else
      {
        setCreateError('Unexpected response from server.');
      }
    }
    catch (err: any)
    {
      // RFC7807 Problem Details
      if (err.response?.data?.errors)
      {
        setFieldErrors(err.response.data.errors);
      }
      else if (err.response?.data?.title)
      {
        setCreateError(err.response.data.title);
      }
      else
      {
        setCreateError('Failed to create board.');
      }
    }
    finally
    {
      setCreating(false);
    }
  };

  return (

    <>
      {/* Search bar and Create Board button */}
      <div className="max-w-4xl mx-auto px-4 pt-8 pb-2 flex items-center gap-4 relative">
        <input
          type="text"
          placeholder="Search boards by title..."
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="w-full px-4 py-2 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-400" />
        <div className="relative">
          <button
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition whitespace-nowrap flex-shrink-0"
            onClick={() => setShowModal((v) => !v)}
          >
            Create board
          </button>
          <CreateBoardDropdown
            open={showModal}
            onClose={() =>
            {
              setShowModal(false);
              setCreateError(null);
              setFieldErrors({});
            }}
            onCreate={handleCreateBoard}
            loading={creating}
            error={createError}
            fieldErrors={fieldErrors} />
        </div>
      </div>

      {isLoading ? (
        <div className="flex justify-center items-center h-64">
          <Spinner />
        </div>
      ) : (
        <>


          <div className="w-full max-w-6xl p-6 mx-auto">
            <h2 className="text-3xl font-bold text-blue-800 mb-6">
              Boards
            </h2>
            {/* {error && <p className="text-red-600 mb-4 text-center">{error}</p>} */}
            {boards.length > 0 ? (
              <ul className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 justify-items-center cursor-pointer">
                {filteredBoards.map((board) => (
                  <li
                    key={board.id}
                    className="w-full max-w-xs"
                  >
                    {/* Card container with clickable navigation, plus an ellipsis button for settings */}
                    <div
                      onClick={() => navigate(`board/${board.id}`)}
                      role="button"
                      tabIndex={0}
                      className="group w-full h-full bg-white rounded-lg shadow hover:shadow-lg transition-transform duration-200 hover:scale-105 flex flex-col items-stretch text-left p-0 overflow-hidden"
                      style={{ minHeight: '220px' }}
                    >
                      {/* Wallpaper or fallback color */}
                      {board.wallpaperImageId ? (
                        <div className="h-32 w-full bg-gray-200">
                          <img
                            src={`/api/wallpapers/${board.wallpaperImageId}`}
                            alt="Board wallpaper"
                            className="w-full h-full object-cover" />
                        </div>
                      ) : (
                        <div className="h-32 w-full base-gradient-bg" />
                      )}
                      {/* Title row with ellipsis button */}
                      <div className="flex-1 flex flex-col px-4 py-3">
                        <div className="flex items-start justify-between gap-2 mb-1">
                          <div className="text-xl font-semibold text-blue-800 truncate">
                            {board.title}
                          </div>
                          <button
                            className="text-gray-500 hover:text-gray-700 rounded p-2 min-w-8 min-h-8 flex items-center justify-center -mr-1 cursor-pointer transition-colors duration-150 hover:bg-gray-200"
                            aria-label="Open board settings"
                            ref={el => { ellipsisRefs.current[board.id] = el; }}
                            onClick={e =>
                            {
                              e.stopPropagation();
                              const btn = ellipsisRefs.current[board.id];
                              if (btn)
                              {
                                const rect = btn.getBoundingClientRect();
                                // Position relative to viewport, adjust for scroll and panel width
                                setSettingsPanelPosition({
                                  top: rect.bottom + window.scrollY + 8, // 8px offset
                                  right: window.innerWidth - rect.right - 43
                                });
                              } else
                              {
                                setSettingsPanelPosition(null);
                              }
                              setOpenSettingsBoardId(board.id);
                            }}
                          >
                            ⋮
                          </button>
                        </div>
                        {/* Description */}
                        <div className="text-sm text-gray-500 overflow-hidden" style={{ display: '-webkit-box', WebkitLineClamp: 2, WebkitBoxOrient: 'vertical' }}>
                          {board.description || "No description"}
                        </div>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            ) : search.trim() === '' ? (
              <p className="text-gray-600 text-center">No boards found. Create one now 😎🤙</p>
            ) : (
              <p className="text-gray-600 text-center">No boards found matching "{search}"</p>
            )}
          </div>
        </>
      )}

      {/* Settings panel overlay */}
      {openSettingsBoardId && (() =>
      {
        const board = boards.find(b => b.id === openSettingsBoardId);
        if (!board) return null;
        return (
          <BoardSettingsPanel
            onClose={() =>
            {
              setOpenSettingsBoardId(null);
              setSettingsPanelPosition(null);
            }}
            position={settingsPanelPosition || undefined}
            boardId={board.id}
            title={board.title}
            description={board.description}
            onUpdated={updated =>
            {
              setBoards(prev =>
                prev.map(b =>
                  b.id === updated.id
                    ? { ...b, title: updated.title, description: updated.description }
                    : b
                )
              );
            }}
          />
        );
      })()}
    </>
  );
}

export default Boards;