import { useKeycloak } from '@react-keycloak/web';
import { useEffect, useState, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import CreateBoardDropdown from '../components/CreateBoardDropdown';
import Spinner from '../components/Spinner';
import api from '../api';
import BoardSettingsPanel from '../components/BoardSettingsPanel';
import toast from 'react-hot-toast';
import { HubConnectionState } from '@microsoft/signalr';
import { useBoardSignalR } from '../communication/BoardSignalRProvider';
import { useTranslation } from 'react-i18next';

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
  const { t } = useTranslation();
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

  const boardHubConnection = useBoardSignalR();
  useEffect(() =>
  {
    const connect = async () =>
    {
      try
      {
        if (initialized && keycloak.authenticated && keycloak.token && boardHubConnection)
        {
          if (boardHubConnection.state === HubConnectionState.Disconnected)
          {
            await boardHubConnection.start();
            console.log('Connection started');
          }
        }
      }
      catch (err)
      {
        console.error('Error during connection:', err);
      }
    };
    connect();
  }, [initialized, keycloak.authenticated, keycloak.token, boardHubConnection]);

  // Track joined boards
  const joinedBoardsRef = useRef<Set<string>>(new Set());

  // Join/leave groups when boards change
  useEffect(() =>
  {
    if (
      boardHubConnection.state !== HubConnectionState.Connected ||
      !initialized ||
      !keycloak.authenticated ||
      !keycloak.token
    ) return;

    const currentBoardIds = new Set(boards.map(b => b.id));
    const prevBoardIds = joinedBoardsRef.current;

    // Join new boards
    for (const id of currentBoardIds)
    {
      if (!prevBoardIds.has(id))
      {
        boardHubConnection.invoke('JoinGroup', id)
          .then(() => console.log('Joined board group: ' + id))
          .catch(err => console.error('Error joining group:', err));
        prevBoardIds.add(id);
      }
    }
    // Leave removed boards
    for (const id of Array.from(prevBoardIds))
    {
      if (!currentBoardIds.has(id))
      {
        boardHubConnection.invoke('LeaveGroup', id)
          .then(() => console.log('Left board group: ' + id))
          .catch(err => console.error('Error leaving group:', err));
        prevBoardIds.delete(id);
      }
    }

    // Cleanup: leave all joined boards on unmount
    return () =>
    {
      for (const id of Array.from(joinedBoardsRef.current))
      {
        boardHubConnection.invoke('LeaveGroup', id)
          .then(() => console.log('Left board group: ' + id))
          .catch(err => console.error('Error leaving group:', err));
      }
      joinedBoardsRef.current.clear();
    };
  }, [boards, initialized, keycloak.authenticated, keycloak.token]);

  // Effect: Listen for BoardUpdated and BoardDeleted events and update board data
  useEffect(() =>
  {
    const handleBoardUpdated = async (updatedBoardId: string) =>
    {
      if (boards.some(b => b.id === updatedBoardId))
      {
        try
        {
          const res = await api.get(`/api/boards/${updatedBoardId}`);
          setBoards(prev =>
            prev.map(b =>
              b.id === updatedBoardId
                ? { ...b, ...res.data }
                : b
            )
          );
        }
        catch
        {
        }
      }
    };

    const handleBoardDeleted = (deletedBoardId: string) =>
    {
      if (boards.some(b => b.id === deletedBoardId))
      {
        setBoards(prev => prev.filter(b => b.id !== deletedBoardId));
      }
    };

    boardHubConnection.on('BoardUpdated', handleBoardUpdated);
    boardHubConnection.on('BoardDeleted', handleBoardDeleted);

    // Cleanup: Remove listeners on unmount
    return () =>
    {
      boardHubConnection.off('BoardUpdated', handleBoardUpdated);
      boardHubConnection.off('BoardDeleted', handleBoardDeleted);
    };
  }, [boards]);

  // filtering boards by title and then description(if not null)
  const filteredBoards = boards.filter(board =>
    board.title.toLowerCase().includes(search.toLowerCase()) ||
    (board.description && board.description.toLowerCase().includes(search.toLowerCase()))
  );

  // Add wallpaper state: boardId -> image URL
  const [boardWallpapers, setBoardWallpapers] = useState<{ [boardId: string]: string }>({});

  // Fetch wallpapers for boards
  useEffect(() =>
  {
    let isMounted = true;
    const fetchWallpapers = async () =>
    {
      const wallpaperMap: { [boardId: string]: string } = {};
      await Promise.all(boards
        .filter(board => !!board.wallpaperImageId) // Only boards with wallpaperImageId
        .map(async board =>
        {
          try
          {
            // Fetch media array for this board
            const res = await api.get(`/api/media/${board.wallpaperImageId}`);
            if (res.status === 200 && res.data && res.data.data)
            {
              // Decode base64 string to binary
              const byteString = atob(res.data.data);
              const byteArray = new Uint8Array(byteString.length);
              for (let i = 0; i < byteString.length; i++)
                byteArray[i] = byteString.charCodeAt(i);
              const blob = new Blob([byteArray], { type: 'image/jpeg' });
              wallpaperMap[board.id] = URL.createObjectURL(blob);
            }
          }
          catch
          {
            // No wallpaper for this board, fallback
          }
        }));
      if (isMounted)
        setBoardWallpapers(wallpaperMap);
    };
    if (boards.length > 0)
      fetchWallpapers();
    else
      setBoardWallpapers({});
    return () =>
    {
      isMounted = false;
      Object.values(boardWallpapers).forEach(url => URL.revokeObjectURL(url));
    };
  }, [boards]);

  // Create board handler
  const handleCreateBoard = async (data: { title: string; description?: string; wallpaperImageId?: string }) =>
  {
    setCreateError(null);

    setCreating(true);
    try
    {
      const payload: any = { title: data.title };
      if (data.description) payload.description = data.description;

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

  useEffect(() =>
  {
    // Listen for board invite event and refetch boards
    const handleInvited = async () =>
    {
      // Refetch boards
      if (initialized && keycloak.authenticated && keycloak.token)
      {
        setIsLoading(true);
        await api.get('/api/boards')
          .then(response =>
          {
            setBoards(response.data);
            setError(null);
          })
          .catch(error =>
          {
            console.error('Failed to fetch boards:', error);
            setError('Failed to load boards. Please try again later.');
          })
          .finally(() =>
          {
            setIsLoading(false);
          });
      }
    };
    // Listen for board removed event and refetch boards
    const handleRemoved = async () =>
    {
      if (initialized && keycloak.authenticated && keycloak.token)
      {
        setIsLoading(true);
        await api.get('/api/boards')
          .then(response =>
          {
            setBoards(response.data);
            setError(null);
          })
          .catch(error =>
          {
            console.error('Failed to fetch boards:', error);
            setError('Failed to load boards. Please try again later.');
          })
          .finally(() =>
          {
            setIsLoading(false);
          });
      }
    };
    window.addEventListener("boardis:invited", handleInvited);
    window.addEventListener("boardis:removed", handleRemoved);
    return () =>
    {
      window.removeEventListener("boardis:invited", handleInvited);
      window.removeEventListener("boardis:removed", handleRemoved);
    };
  }, [initialized, keycloak.authenticated, keycloak.token]);

  return (

    <>
      {/* Search bar and Create Board button */}
      <div className="max-w-4xl mx-auto px-4 pt-8 pb-2 flex items-center gap-4 relative">
        <input
          type="text"
          placeholder={t('boardsSearchPlaceholder')}
          value={search}
          onChange={e => setSearch(e.target.value)}
          className="w-full px-4 py-2 rounded-md border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-400" />
        <div className="relative">
          <button
            className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 transition whitespace-nowrap flex-shrink-0"
            onClick={() => setShowModal((v) => !v)}
          >
            {t('boardsCreateButton')}
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
              {t('boardsTitle')}
            </h2>
            {/* {error && <p className="text-red-600 mb-4 text-center">{error}</p>} */}
            {filteredBoards.length > 0 ? (
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
                      {boardWallpapers[board.id] ? (
                        <div className="h-32 w-full bg-gray-200">
                          <img
                            src={boardWallpapers[board.id]}
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
                            aria-label={t('boardsSettingsAria')}
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
                          {board.description || t('boardsNoDescription')}
                        </div>
                      </div>
                    </div>
                  </li>
                ))}
              </ul>
            ) : search.trim() === '' ? (
              <p className="text-gray-600 text-center">{t('boardsNoBoards')}</p>
            ) : (
              <p className="text-gray-600 text-center">{t('boardsNoBoardsMatching', { search })}</p>
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
            onDeleted={() =>
            {
              setBoards(prev => prev.filter(b => b.id !== board.id));
              setOpenSettingsBoardId(null);
              setSettingsPanelPosition(null);
              toast.success(t('boardsDeleteSuccess'));
            }}
          />
        );
      })()}
    </>
  );
}

export default Boards;