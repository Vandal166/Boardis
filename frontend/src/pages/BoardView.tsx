import { useKeycloak } from '@react-keycloak/web';
import axios from 'axios';
import { useParams, useNavigate } from 'react-router-dom';
import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core';
import { arrayMove, SortableContext, sortableKeyboardCoordinates, rectSortingStrategy } from '@dnd-kit/sortable';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Cog6ToothIcon, EllipsisHorizontalIcon, UserPlusIcon } from '@heroicons/react/24/solid';
import Spinner from '../components/Spinner';
import BoardCards from '../components/BoardCards';
import AddListButton from '../components/AddListButton';
import { useBoardLists } from '../hooks/userBoardLists';
import { useEffect, useRef, useState } from 'react';
import AddCardButton from '../components/AddCardButton';
import { useUserListCards } from '../hooks/userListCard';
import BoardSettingsPanel from '../components/BoardSettingsPanel';
import BoardAddMemberModal from '../components/ManageBoardMembersModal';
import toast from 'react-hot-toast';

interface BoardList
{
  id: string;
  boardId: string;
  title: string;
  position: number;
  colorArgb: number;
}

const argbToRgba = (color: number) =>
{
  const a = (color >> 24) & 0xFF;
  const r = (color >> 16) & 0xFF;
  const g = (color >> 8) & 0xFF;
  const b = color & 0xFF;
  return `rgba(${r},${g},${b},${a / 255})`;
};

const SortableList = ({ list }: { list: BoardList }) =>
{
  const { attributes, listeners, setNodeRef, transform, transition } = useSortable({ id: list.id });
  const [showMenu, setShowMenu] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  const [addingCard, setAddingCard] = useState(false);
  const [newCardTitle, setNewCardTitle] = useState('');

  const { boardId } = useParams<{ boardId: string }>();
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();

  const {
    cards,
    setFieldErrors: setCardFieldErrors,
    handleAddCard,
    error: cardError,
    fieldErrors: cardFieldErrors,
  } = useUserListCards(boardId, list.id, keycloak, navigate, initialized);

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
  };

  useEffect(() =>
  {
    if (!showMenu) return;
    function handleClick(e: MouseEvent)
    {
      if (menuRef.current && !menuRef.current.contains(e.target as Node))
      {
        setShowMenu(false);
      }
    }
    document.addEventListener('mousedown', handleClick);
    return () => document.removeEventListener('mousedown', handleClick);
  }, [showMenu]);


  return (
    <div ref={setNodeRef} style={style} {...attributes} className="bg-gray-100 rounded-lg p-4 flex flex-col w-[300px] min-w-[300px] max-w-[300px] min-h-[220px]">
      <div
        className="flex mb-4 p-2 rounded-t-lg cursor-grab relative"
        {...listeners}
        style={{ backgroundColor: argbToRgba(list.colorArgb) }}
      >
        <h3 className="text-lg font-semibold text-black break-words whitespace-normal line-clamp-2 max-w-full pr-8">
          {list.title}
        </h3>
        <div className="absolute top-2 right-2">
          <button
            aria-label="List settings"
            onPointerDown={e => e.stopPropagation()}
            onClick={e =>
            {
              e.stopPropagation();
              setShowMenu((v) => !v);
            }}
            className="p-1 rounded hover:bg-white/20 transition"
          >
            <EllipsisHorizontalIcon className="w-5 h-5 text-white" />
          </button>
          {showMenu && (
            <div
              ref={menuRef}
              className="absolute left-0 mt-2 w-52 bg-white rounded-xl shadow-xl z-50 py-2 border border-gray-220"
            >
              {/* Arrow */}
              <div className="absolute -top-2 left-3 w-4 h-4 z-10">
                <div className="w-4 h-4 bg-white rotate-45 shadow-lg border-t border-l border-gray-200" />
              </div>
              <button
                className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
              >
                <span className="w-2 h-2 rounded-full bg-blue-400"></span>
                Change list color
              </button>
              <button
                className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
              >
                <span className="w-2 h-2 rounded-full bg-gray-400"></span>
                More info
              </button>
              <button
                className="flex items-center gap-2 w-full text-left px-5 py-2 text-red-600 hover:bg-red-50 hover:border-l-4 hover:border-red-500 transition"
              >
                <span className="w-2 h-2 rounded-full bg-red-400"></span>
                Delete list
              </button>
            </div>
          )}
        </div>
      </div>

      <BoardCards cards={cards} />

      {addingCard ? (
        <AddCardButton
          value={newCardTitle}
          error={cardError}
          fieldErrors={cardFieldErrors}
          onChange={setNewCardTitle}
          onAdd={async (e) =>
          {
            e.preventDefault();
            const success = await handleAddCard({ title: newCardTitle });
            if (success)
            {
              setAddingCard(false);
              setNewCardTitle('');
              setCardFieldErrors({});
            }
          }}
          onCancel={() =>
          {
            setAddingCard(false);
            setNewCardTitle('');
            setCardFieldErrors({});
          }}
        />
      ) : (
        <button
          className="mt-4 text-blue-600 hover:underline text-left"
          onClick={() => setAddingCard(true)}
        >
          + Add a card
        </button>
      )}
    </div>
  );
};

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

  const [members, setMembers] = useState<{ id: string; username: string; email: string; role: string }[]>([]);
  const [membersLoading, setMembersLoading] = useState(true);

  useEffect(() =>
  {
    if (!keycloak.token) return;
    axios.get('/api/roles', {
      headers: { Authorization: `Bearer ${keycloak.token}` }
    })
      .then(res => setRoles(res.data))
      .catch(() => setRoles([]));
  }, [keycloak.token]);

  useEffect(() =>
  {
    if (!boardId || !keycloak.token) return;
    setMembersLoading(true);
    axios.get(`/api/boards/${boardId}/members`, {
      headers: { Authorization: `Bearer ${keycloak.token}` }
    })
      .then(res => setMembers(res.data))
      .catch(() => setMembers([]))
      .finally(() => setMembersLoading(false));
  }, [boardId, keycloak.token]);


  const handleAddMember = async (emailOrUsername: string, role: string) =>
  {
    if (!boardId || !keycloak.token)
      return;

    try
    {
      // 1. Lookup user by email or username
      let userRes;
      const by = emailOrUsername.includes('@') ? 'email' : 'username';
      userRes = await axios.get(`/api/users/${by}/${encodeURIComponent(emailOrUsername)}`, {
        headers: { Authorization: `Bearer ${keycloak.token}` }
      });

      const user = userRes.data;
      if (!user?.id)
      {
        toast.error('User not found.');
        return;
      }

      // 2. Add user as board member
      await axios.post(
        `/api/boards/${boardId}/members`,
        {
          userId: user.id,
          role: role
        },
        { headers: { Authorization: `Bearer ${keycloak.token}` } }
      );

      // 3. Refresh members list
      setMembersLoading(true);
      const membersRes = await axios.get(`/api/boards/${boardId}/members`, {
        headers: { Authorization: `Bearer ${keycloak.token}` }
      });
      setMembers(membersRes.data);
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
          axios.put(
            `/api/boards/${boardId}/lists/${list.id}`,
            {
              title: list.title,
              position: list.position,
              colorArgb: list.colorArgb,
            },
            { headers: { Authorization: `Bearer ${keycloak.token}` } }
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
                  clearErrors={() =>
                  {
                    setFieldErrors({});
                  }}
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