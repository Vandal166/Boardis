import { useEffect, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { EllipsisHorizontalIcon } from '@heroicons/react/24/solid';

import BoardCards from './BoardCards';
import AddCardButton from './AddCardButton';
import { useUserListCards } from '../hooks/userListCard';

export interface BoardList
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

function SortableList({ list }: { list: BoardList })
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
}

export default SortableList;