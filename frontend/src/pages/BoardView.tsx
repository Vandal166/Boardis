import { useKeycloak } from '@react-keycloak/web';
import { useParams, useNavigate } from 'react-router-dom';
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core';
import { SortableContext, sortableKeyboardCoordinates, rectSortingStrategy } from '@dnd-kit/sortable';

import { Cog6ToothIcon, UserPlusIcon } from '@heroicons/react/24/solid';
import Spinner from '../components/Spinner';
import AddListButton from '../components/AddListButton';
import { useBoardLists } from '../hooks/userBoardLists';
import { useCallback, useEffect, useMemo, useState } from 'react';
import BoardSettingsPanel from '../components/BoardSettingsPanel';
import ManageBoardMembersModal from '../components/ManageBoardMembersModal';
import toast from 'react-hot-toast';
import SortableList from '../components/SortableList';
import api from '../api';
import { HubConnectionState } from '@microsoft/signalr';
import { useBoardSignalR } from '../communication/BoardSignalRProvider';


function BoardView()
{
  const { boardId } = useParams<{ boardId: string }>();
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const [showSettings, setShowSettings] = useState(false);
  const [showAddMember, setShowAddMember] = useState(false);
  // Board info for settings panel
  const [boardInfo, setBoardInfo] = useState<{ id: string; title: string; description?: string } | null>(null);

  const {
    lists,
    setLists,
    error,
    isLoading,
    fieldErrors,
    setFieldErrors,
    handleCreateList,
  } = useBoardLists(boardId, keycloak, navigate, initialized);

  const boardHubConnection = useBoardSignalR();
  useEffect(() =>
  {
    // Defer connection/join until Keycloak is ready
    if (!initialized || !keycloak.authenticated || !keycloak.token || !boardId)
      return;

    const connectAndJoin = async () =>
    {
      try
      {
        if (boardHubConnection.state === HubConnectionState.Disconnected)
        {
          await boardHubConnection.start();
          console.log('Connection started');
        }

        await boardHubConnection.invoke('JoinGroup', boardId);
        console.log('Joined board group: ' + boardId);
      }
      catch (err)
      {
        console.error('Error during connection or join:', err);
      }
    };

    connectAndJoin();

    // event listeners
    const handleBoardUpdated = async (updatedBoardId: string) =>
    {
      if (updatedBoardId === boardId)
      {
        // Refetch board info and update state
        await api.get(`/api/boards/${boardId}`)
          .then(res => setBoardInfo(res.data))
          .catch(() => setBoardInfo(null));
      }
    };

    const handleBoardDeleted = (deletedBoardId: string) =>
    {
      if (deletedBoardId === boardId)
      {
        toast.success('This board has just been deleted by the owner.');
        navigate('/dashboard');
      }
    };

    boardHubConnection.on('BoardUpdated', handleBoardUpdated);
    boardHubConnection.on('BoardDeleted', handleBoardDeleted);

    boardHubConnection.on('BoardListCreated', async (updatedBoardId: string) =>
    {
      if (boardId === updatedBoardId)
      {
        console.log('BoardListCreated event received for board ' + updatedBoardId);
        // Refetch lists
        await api.get(`/api/boards/${boardId}/lists`)
          .then(res => setLists(res.data))
          .catch(() => setLists([]));
      }
    });

    boardHubConnection.on('BoardListUpdated', async (updatedBoardId: string, updatedListId: string) =>
    {
      if (boardId === updatedBoardId)
      {
        console.log('BoardListUpdated event received for board ' + updatedBoardId + ' list ' + updatedListId);
        // Fetch only the updated list
        await api.get(`/api/boards/${boardId}/lists/${updatedListId}`)
          .then(res =>
          {
            setLists(prev =>
              [...prev.map(l => l.id === updatedListId ? res.data : l)]
                .sort((a, b) => a.position - b.position)
            );
          });
      }
    });

    boardHubConnection.on('BoardListDeleted', async (updatedBoardId: string, deletedListId: string) =>
    {
      if (boardId === updatedBoardId)
      {
        console.log('BoardListDeleted event received for board ' + updatedBoardId + ' list ' + deletedListId);
        //delete from local state
        setLists(prev => prev.filter(l => l.id !== deletedListId));
      }
    });

    // Cleanup: Leave group and remove listeners on unmount
    return () =>
    {
      if (boardHubConnection.state === HubConnectionState.Connected)
      {
        boardHubConnection.invoke('LeaveGroup', boardId)
          .then(() => console.log('Left board group: ' + boardId))
          .catch((err: any) => console.error('Error leaving group:', err));
      }
      boardHubConnection.off('BoardUpdated', handleBoardUpdated);
      boardHubConnection.off('BoardDeleted', handleBoardDeleted);
      boardHubConnection.off('BoardListCreated');
      boardHubConnection.off('BoardListUpdated');
      boardHubConnection.off('BoardListDeleted');
    };
  }, [boardId, boardHubConnection, navigate, initialized, keycloak.authenticated, keycloak.token]);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const [members, setMembers] = useState<{ userId: string; username: string; email: string; permissions?: string[] }[]>([]);
  const [membersLoading, setMembersLoading] = useState(true);

  // Instead, just use list ids for sortableItems
  const sortableItems = useMemo(
    () => lists.map(l => l.id),
    [lists]
  );

  const fetchMembers = async () =>
  {
    if (!boardId || !initialized || !keycloak.authenticated || !keycloak.token) return;

    try
    {
      setMembersLoading(true);
      const res = await api.get(`/api/boards/${boardId}/members`);
      const normalized = (Array.isArray(res.data) ? res.data : []).map((m: any) => ({
        userId: m.UserId || m.userId,
        username: m.Username || m.username,
        email: m.Email || m.email,
        permissions: m.Permissions || m.permissions || []
      }));
      setMembers(normalized);
    }
    catch
    {
      setMembers([]);
    }
    finally
    {
      setMembersLoading(false);
    }
  };


  // Fetch members only when modal opens
  useEffect(() =>
  {
    if (showAddMember)
    {
      void fetchMembers();
    }
  }, [showAddMember]);


  const handleAddMember = async (emailOrUsername: string) =>
  {
    if (!boardId || !keycloak.token)
      return;

    try
    {
      // 1. Lookup user by email or username
      let userRes;
      const by = emailOrUsername.includes('@') ? 'email' : 'username';
      userRes = await api.get(`/api/users/${by}/${encodeURIComponent(emailOrUsername)}`);

      const user = userRes.data;
      if (!user?.id)
      {
        toast.error('User not found.');
        return;
      }

      // 2. Add user as board member
      await api.post(
        `/api/boards/${boardId}/members`,
        {
          userId: user.id
        }
      );

      // 3. Refresh members list
      await fetchMembers();
      toast.success('Member added successfully!');
    }
    catch (err: any)
    {
      if (err?.response?.status !== 403)
      {
        const msg =
          (err.response?.data && (err.response.data.detail || err.response.data.title || err.response.data.message)) ||
          'You do not have permission to perform this action.';

        toast.error(msg);
      }
    }
    finally
    {
      setMembersLoading(false);
    }
  };


  const handleRemoveMember = async (memberId: string) =>
  {
    if (!boardId || !keycloak.token)
      return;

    try
    {
      await api.delete(`/api/boards/${boardId}/members/${memberId}`);
      setMembers(members => members.filter(m => m.userId !== memberId));
      toast.success('Member removed successfully!');
    }
    catch (err: any)
    {
      if (err?.response?.status !== 403)
      {
        const msg =
          (err.response?.data && (err.response.data.detail || err.response.data.title || err.response.data.message)) ||
          'You do not have permission to perform this action.';

        toast.error(msg);
      }
    }
    finally
    {
      setMembersLoading(false);
    }
  };


  const handleDragEnd = useCallback(async (event: DragEndEvent) =>
  {
    const { active, over } = event;
    if (!over || active.id === over.id || !keycloak.token || !boardId) return;

    const prevLists = lists;
    // Dropped over another list: compute midpoint position for moved list
    const sourceIdx = prevLists.findIndex(l => l.id === active.id);
    const targetIdx = prevLists.findIndex(l => l.id === over.id);
    if (sourceIdx === -1 || targetIdx === -1 || sourceIdx === targetIdx) return;

    // Remove source, insert at target
    const newLists = [...prevLists];
    const [movedList] = newLists.splice(sourceIdx, 1);
    newLists.splice(targetIdx, 0, movedList);

    // Compute midpoint position for moved list
    const prevList = targetIdx > 0 ? newLists[targetIdx - 1] : null;
    const nextList = targetIdx < newLists.length - 1 ? newLists[targetIdx + 1] : null;

    let newPosition: number;
    if (!prevList && !nextList)
    {
      newPosition = 1024.0;
    }
    else if (!prevList)
    {
      newPosition = nextList!.position / 2;
    }
    else if (!nextList)
    {
      newPosition = prevList.position + 1024.0;
    }
    else
    {
      newPosition = (prevList.position + nextList.position) / 2;
    }

    movedList.position = newPosition;
    const sorted = [...newLists].sort((a, b) => a.position - b.position);
    setLists(sorted);

    try
    {
      const patchOps = [
        { op: 'replace', path: '/position', value: movedList.position }
      ];

      if (patchOps.length === 0)
        return;

      await api.patch(
        `/api/boards/${boardId}/lists/${movedList.id}`,
        patchOps,
        { headers: { 'Content-Type': 'application/json-patch+json' } }
      );
    }
    catch
    {
      setLists(prevLists); // revert snapshot
    }
  }, [lists, keycloak.token, boardId]);

  useEffect(() =>
  {
    if (!boardId || !initialized || !keycloak.authenticated || !keycloak.token) return;
    api.get(`/api/boards/${boardId}`)
      .then(res => setBoardInfo(res.data))
      .catch(() => setBoardInfo(null));
  }, [boardId, initialized, keycloak.authenticated, keycloak.token]);

  useEffect(() =>
  {
    const handleRemoved = (e: any) =>
    {
      const notification = e.detail;
      if (notification?.boardId === boardId)
      {
        toast.error(notification.title + ` by ${notification.byUser}`, { duration: 8000 });
        navigate('/dashboard');
      }
    };
    window.addEventListener("boardis:removed", handleRemoved);
    return () => window.removeEventListener("boardis:removed", handleRemoved);
  }, [boardId, navigate]);

  // Wallpaper state for this board
  const [wallpaperUrl, setWallpaperUrl] = useState<string | null>(null);

  useEffect(() =>
  {
    let isMounted = true;
    const fetchWallpaper = async () =>
    {
      // Defer API call until Keycloak is ready to avoid 401 redirect
      if (!boardId || !initialized || !keycloak.authenticated || !keycloak.token)
      {
        if (isMounted) setWallpaperUrl(null);
        return;
      }
      try
      {
        // Fetch media array for this board
        const res = await api.get(`/api/media/${boardId}`);
        if (res.status === 200 && Array.isArray(res.data) && res.data.length > 0)
        {
          const media = res.data[0];
          if (media.data)
          {
            // Decode base64 string to binary
            const byteString = atob(media.data);
            const byteArray = new Uint8Array(byteString.length);
            for (let i = 0; i < byteString.length; i++)
              byteArray[i] = byteString.charCodeAt(i);
            const blob = new Blob([byteArray], { type: 'image/jpeg' });
            const url = URL.createObjectURL(blob);
            if (isMounted) setWallpaperUrl(url);
          }
          else if (isMounted) setWallpaperUrl(null);
        }
        else if (isMounted) setWallpaperUrl(null);
      }
      catch
      {
        if (isMounted) setWallpaperUrl(null);
      }
    };
    fetchWallpaper();
    return () =>
    {
      isMounted = false;
      if (wallpaperUrl) URL.revokeObjectURL(wallpaperUrl);
    };
  }, [boardId, initialized, keycloak.authenticated, keycloak.token]);

  if (isLoading)
  {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (error)
  {
    return <div className="min-h-screen flex items-center justify-center text-red-600">{error}</div>;
  }

  return (
    <div className="min-h-screen text-gray-900 font-sans relative">
      {/* Wallpaper background */}
      {wallpaperUrl ? (
        <img
          src={wallpaperUrl}
          alt="Board wallpaper"
          className="absolute inset-0 w-full h-full object-cover z-0"
          style={{ minHeight: '100%', minWidth: '100%' }}
        />
      ) : (
        <div className="absolute inset-0 w-full h-full base-gradient-bg z-0" />
      )}
      {/* Overlay content */}
      <div className="relative z-10 min-h-screen bg-transparent bg-opacity-70">
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <div className="w-full p-6">
            {/* Top bar with Add Member and Settings */}
            <div className="flex justify-between items-center mb-4">
              <button
                className="flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
                onClick={() => setShowAddMember(true)}
              >
                <UserPlusIcon className="w-5 h-5" />
                Manage members
              </button>
              <button
                className="p-2 rounded-full hover:bg-gray-200 transition"
                onClick={() => setShowSettings(v => !v)}
                aria-label="Board settings"
              >
                <Cog6ToothIcon className="w-7 h-7 text-gray-700" />
              </button>
            </div>

            {/* Add Member Modal */}
            {showAddMember && (
              <>
                <ManageBoardMembersModal
                  boardId={boardId!}
                  onClose={() => setShowAddMember(false)}
                  members={members}
                  onAdd={handleAddMember}
                  onRemove={handleRemoveMember}
                  isLoading={membersLoading}
                  fetchMembers={fetchMembers}
                />
              </>
            )}

            {/* Board Settings Panel */}
            {showSettings && boardInfo && (
              <BoardSettingsPanel
                onClose={() => setShowSettings(false)}
                boardId={boardInfo.id}
                title={boardInfo.title}
                description={boardInfo.description}
                onDeleted={() =>
                {
                  toast.success('Board deleted successfully.');
                  navigate('/dashboard');
                }}
              />
            )}

            <div className="max-w-[1664px] mx-auto">
              <SortableContext
                items={sortableItems}
                strategy={rectSortingStrategy}
              >
                <div className="grid grid-cols-5 gap-4 auto-rows-fr">
                  {/* Only render actual lists, no empty slots */}
                  {lists.map((list) => (
                    <SortableList
                      key={list.id}
                      list={list}
                      onDeleted={() => setLists(prev => prev.filter(l => l.id !== list.id))}
                      onTitleUpdated={(newTitle) =>
                        setLists(prev => prev.map(l => l.id === list.id ? { ...l, title: newTitle } : l))
                      }
                      onColorUpdated={(newColor) =>
                        setLists(prev => prev.map(l => l.id === list.id ? { ...l, colorArgb: newColor } : l))
                      }
                    />
                  ))}
                  <AddListButton
                    onCreate={handleCreateList}
                    error={error}
                    fieldErrors={fieldErrors}
                    clearErrors={() => setFieldErrors({})}
                  />
                </div>
              </SortableContext>
            </div>
          </div>
        </DndContext>
      </div>
    </div>
  );
}

export default BoardView;