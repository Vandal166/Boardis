import { useKeycloak } from '@react-keycloak/web';
import { useParams, useNavigate } from 'react-router-dom';
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core';
import { arrayMove, SortableContext, sortableKeyboardCoordinates, rectSortingStrategy } from '@dnd-kit/sortable';

import { Cog6ToothIcon, UserPlusIcon } from '@heroicons/react/24/solid';
import Spinner from '../components/Spinner';
import AddListButton from '../components/AddListButton';
import { useBoardLists } from '../hooks/userBoardLists';
import { useEffect, useState } from 'react';
import BoardSettingsPanel from '../components/BoardSettingsPanel';
import BoardAddMemberModal from '../components/ManageBoardMembersModal';
import toast from 'react-hot-toast';
import SortableList from '../components/SortableList';
import api from '../api';


function BoardView()
{
  const { boardId } = useParams<{ boardId: string }>();
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const [showSettings, setShowSettings] = useState(false);
  const [showAddMember, setShowAddMember] = useState(false);

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

  const [members, setMembers] = useState<{ id: string; username: string; email: string; role: string; }[]>([]);
  const [membersLoading, setMembersLoading] = useState(true);

  useEffect(() =>
  {
    if (!keycloak.token) return;
    api.get('/api/roles')
      .then(res => setRoles(res.data))
      .catch(() => setRoles([]));
  }, [keycloak.token]);

  const fetchMembers = async () =>
  {
    if (!boardId || !keycloak.token) return;
    try
    {
      setMembersLoading(true);
      const res = await api.get(`/api/boards/${boardId}/members`);
      setMembers(res.data);
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



  const handleDragEnd = async (event: DragEndEvent) =>
  {
    const { active, over } = event;

    if (!over || active.id === over.id || !keycloak.token || !boardId) return;

    const oldIndex = lists.findIndex((list) => list.id === active.id);
    const newIndex = lists.findIndex((list) => list.id === over.id);

    const newLists = arrayMove(lists, oldIndex, newIndex);

    const updatedLists = newLists.map((list, index) => ({ ...list, position: index + 1 }));

    setLists(updatedLists);

    // Only update lists that have changed
    const changedLists = updatedLists.filter((updated) =>
    {
      const original = lists.find(l => l.id === updated.id);
      return (
        !original ||
        original.position !== updated.position ||
        original.title !== updated.title ||
        original.colorArgb !== updated.colorArgb
      );
    });

    try
    {
      await Promise.all(
        changedLists.map((list) =>
          api.put(
            `/api/boards/${boardId}/lists/${list.id}`,
            {
              title: list.title,
              position: list.position,
              colorArgb: list.colorArgb,
            }
          )
        )
      );
    }
    catch (error)
    {
      setLists(lists); // Revert on error
    }
  };

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
              <BoardAddMemberModal
                onClose={() => setShowAddMember(false)}
                members={members}
                onAdd={handleAddMember}
                onRemove={id => setMembers(members => members.filter(m => m.id !== id))}
                roles={roles}
                isLoading={membersLoading}
              />
            </>
          )}

          {/* Board Settings Panel */}
          {showSettings && (
            <BoardSettingsPanel onClose={() => setShowSettings(false)} />
          )}

          <div className="max-w-[1664px] mx-auto">
            <SortableContext
              items={lists.map(list => list.id)}
              strategy={rectSortingStrategy}
            >
              <div className="grid grid-cols-5 gap-4 auto-rows-fr">
                {lists
                  .slice()
                  .sort((a, b) => a.position - b.position)
                  .map((list) => (
                    <SortableList key={list.id} list={list} />
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
  );
}

export default BoardView;