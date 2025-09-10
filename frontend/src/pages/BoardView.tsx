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
import EmptySlot from '../components/EmptySlot';


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

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const [roles, setRoles] = useState<{ key: string; displayName: string }[]>([]);

  const [members, setMembers] = useState<{ userId: string; username: string; email: string; role: string; }[]>([]);
  const [membersLoading, setMembersLoading] = useState(true);

  // Build a fast lookup per render
  const listByPos = useMemo(() =>
  {
    const m = new Map<number, typeof lists[number]>();
    for (const l of lists) m.set(l.position, l);
    return m;
  }, [lists]);

  const maxPosition = useMemo(
    () => (lists.length ? Math.max(...lists.map(l => l.position)) : 0),
    [lists]
  );

  const positions = useMemo(
    () => Array.from({ length: maxPosition }, (_, i) => i + 1),
    [maxPosition]
  );

  // Include empty slots to preserve gaps during drag
  const sortableItems = useMemo(
    () => positions.map(pos => listByPos.get(pos)?.id ?? `empty-${pos}`),
    [positions, listByPos]
  );


  useEffect(() =>
  {
    if (!initialized || !keycloak.authenticated || !keycloak.token) return;

    api.get('/api/roles')
      .then(res => setRoles(res.data))
      .catch(() => setRoles([]));
  }, [keycloak.token]);

  const fetchMembers = async () =>
  {
    if (!boardId || !initialized || !keycloak.authenticated || !keycloak.token) return;

    try
    {
      setMembersLoading(true);
      const res = await api.get(`/api/boards/${boardId}/members`);
      setMembers(res.data);
    }
    catch (err: any)
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


  const handleAddMember = async (emailOrUsername: string, role: string) =>
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
          userId: user.id,
          role: role
        }
      );

      // 3. Refresh members list
      await fetchMembers();
      toast.success('Member added successfully!');
    }
    catch (err: any)
    {
      let message = '';
      if (err.response?.data?.errors)
      {
        message = Object.values(err.response.data.errors)
          .flat()
          .join(' ');
      }
      if (!message && err.response?.data?.detail)
      {
        message = err.response.data.detail;
      }
      if (!message && err.response?.data?.title)
      {
        message = err.response.data.title;
      }
      if (!message)
      {
        message = 'Failed to add member. Please try again.';
      }
      toast.error(message);
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
      let message = '';
      if (err.response?.data?.errors)
      {
        message = Object.values(err.response.data.errors)
          .flat()
          .join(' ');
      }
      if (!message && err.response?.data?.detail)
      {
        message = err.response.data.detail;
      }
      if (!message && err.response?.data?.title)
      {
        message = err.response.data.title;
      }
      if (!message)
      {
        message = 'Failed to remove member. Please try again.';
      }
      toast.error(message);
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

    // Dropped on an empty slot: move exactly there (keep gaps)
    if (typeof over.id === 'string' && over.id.startsWith('empty-'))
    {
      const emptyPos = parseInt(over.id.replace('empty-', ''), 10);
      const updated = prevLists.map(l => (l.id === active.id ? { ...l, position: emptyPos } : l));
      const sorted = [...updated].sort((a, b) => a.position - b.position);
      setLists(sorted);

      try
      {
        const moved = sorted.find(l => l.id === active.id);
        if (moved)
        {
          await api.put(`/api/boards/${boardId}/lists/${moved.id}`, {
            title: moved.title, position: moved.position, colorArgb: moved.colorArgb
          });
        }
      }
      catch
      {
        setLists(prevLists); // revert snapshot
      }
      return;
    }

    // Dropped over another list: shift only the range (preserve gaps)
    const source = prevLists.find(l => l.id === active.id);
    const target = prevLists.find(l => l.id === over.id);
    if (!source || !target || source.position === target.position) return;

    const beforePos = new Map(prevLists.map(l => [l.id, l.position]));

    const changed =
      source.position < target.position
        ? prevLists.map(l =>
          l.id === source.id
            ? { ...l, position: target.position }
            : l.position > source.position && l.position <= target.position
              ? { ...l, position: l.position - 1 }
              : l
        )
        : prevLists.map(l =>
          l.id === source.id
            ? { ...l, position: target.position }
            : l.position >= target.position && l.position < source.position
              ? { ...l, position: l.position + 1 }
              : l
        );

    const sorted = [...changed].sort((a, b) => a.position - b.position);
    setLists(sorted);

    try
    {
      const toUpdate = changed.filter(l => l.position !== beforePos.get(l.id));
      await Promise.all(
        toUpdate.map(l =>
          api.put(`/api/boards/${boardId}/lists/${l.id}`, {
            title: l.title, position: l.position, colorArgb: l.colorArgb
          })
        )
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
    <div className="min-h-screen bg-gray-300 text-gray-900 font-sans">
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
                onClose={() => setShowAddMember(false)}
                members={members}
                onAdd={handleAddMember}
                onRemove={handleRemoveMember}
                roles={roles}
                isLoading={membersLoading}
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
            />
          )}

          <div className="max-w-[1664px] mx-auto">
            <SortableContext
              items={sortableItems}
              strategy={rectSortingStrategy}
            >
              <div className="grid grid-cols-5 gap-4 auto-rows-fr">
                {positions.map((pos) =>
                {
                  const list = listByPos.get(pos);
                  return list ? (
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
                  ) : (
                    <EmptySlot key={`empty-${pos}`} id={`empty-${pos}`} />
                  );
                })}
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
  );
}

export default BoardView;