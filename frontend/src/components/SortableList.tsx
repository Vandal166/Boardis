import { useEffect, useMemo, useRef, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useKeycloak } from '@react-keycloak/web';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { EllipsisHorizontalIcon } from '@heroicons/react/24/solid';

import AddCardButton from './AddCardButton';
import { useUserListCards } from '../hooks/userListCard';
import DeleteListButton from './DeleteListButton';
import { useConfirmationDialogOpen } from './ConfirmationDialog';
import api from '../api';
import ListColorPicker from './ListColorPicker';

import { DndContext, closestCenter, KeyboardSensor, PointerSensor, useSensor, useSensors, type DragEndEvent } from '@dnd-kit/core';
import { SortableContext, rectSortingStrategy, sortableKeyboardCoordinates } from '@dnd-kit/sortable';
import SortableCard from './SortableCard';

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

function SortableList({ list, onDeleted, onTitleUpdated, onColorUpdated }: { list: BoardList; onDeleted: () => void; onTitleUpdated: (title: string) => void; onColorUpdated: (color: number) => void; })
{
    const { attributes, listeners, setNodeRef, transform, transition } = useSortable({ id: list.id });
    const [showMenu, setShowMenu] = useState(false);
    const menuRef = useRef<HTMLDivElement>(null);

    const [addingCard, setAddingCard] = useState(false);
    const [newCardTitle, setNewCardTitle] = useState('');

    const { boardId } = useParams<{ boardId: string }>();
    const { keycloak, initialized } = useKeycloak();
    const navigate = useNavigate();
    const confirmationDialogOpen = useConfirmationDialogOpen();

    const [editingTitle, setEditingTitle] = useState(false);
    const [titleInput, setTitleInput] = useState(list.title);
    const [titleLoading, setTitleLoading] = useState(false);
    const [titleError, setTitleError] = useState<string | null>(null);
    const checkmarkClicked = useRef(false);

    const [showColorPicker, setShowColorPicker] = useState(false);
    const [listColor, setListColor] = useState(list.colorArgb);


    const {
        cards,
        setFieldErrors: setCardFieldErrors,
        handleAddCard,
        error: cardError,
        fieldErrors: cardFieldErrors,
        refetch,
    } = useUserListCards(boardId, list.id, keycloak, navigate, initialized);

    // Maintain local ordering for optimistic updates
    const sortedFromHook = useMemo(() => [...cards].sort((a, b) => a.position - b.position), [cards]);
    const [localCards, setLocalCards] = useState(sortedFromHook);
    useEffect(() => setLocalCards(sortedFromHook), [sortedFromHook]);

    // Sensors for inner DnD (cards)
    const sensors = useSensors(
        useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
        useSensor(KeyboardSensor, { coordinateGetter: sortableKeyboardCoordinates })
    );

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
    };

    useEffect(() =>
    {
        if (!showMenu || confirmationDialogOpen) return;
        function handleClick(e: MouseEvent)
        {
            if (menuRef.current && !menuRef.current.contains(e.target as Node))
            {
                setShowMenu(false);
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [showMenu, confirmationDialogOpen]);

    const handleTitleEdit = () =>
    {
        setEditingTitle(true);
        setTitleInput(list.title);
        setTitleError(null);
    };

    const handleTitleSubmit = async () =>
    {
        checkmarkClicked.current = false; // reset
        if (titleInput === list.title)
        {
            setEditingTitle(false);
            setTitleError(null);
            return;
        }
        setTitleLoading(true);
        setTitleError(null);
        try
        {
            const patchOps = [
                { op: 'replace', path: '/title', value: titleInput }
            ];
            await api.patch(
                `/api/boards/${list.boardId}/lists/${list.id}`,
                patchOps,
                { headers: { 'Content-Type': 'application/json-patch+json' } }
            );
            setEditingTitle(false);
            setTitleError(null);
            onTitleUpdated(titleInput);
        }
        catch (err: any)
        {
            let message = 'Failed to update title.';
            if (err.response?.data?.errors?.Title && Array.isArray(err.response.data.errors.Title))
            {
                message = err.response.data.errors.Title[0];
            } else if (err.response?.data?.detail)
            {
                message = err.response.data.detail;
            } else if (err.response?.data?.title)
            {
                message = err.response.data.title;
            }
            setTitleError(message);
        }
        finally
        {
            setTitleLoading(false);
        }
    };

    // Only close input if checkmark wasn't clicked
    const handleInputBlur = () =>
    {
        if (checkmarkClicked.current)
        {
            checkmarkClicked.current = false;
            return;
        }
        setEditingTitle(false);
    };
    const handleColorChanged = (color: number) =>
    {
        setListColor(color);
        onColorUpdated(color);
    };

    // Card drag end: compute midpoint for moved card and update server with batch (single item)
    const handleCardDragEnd = async (event: DragEndEvent) =>
    {
        const { active, over } = event;
        if (!over || active.id === over.id) return;

        setLocalCards((prev) =>
        {
            const newCards = [...prev];
            const oldIndex = newCards.findIndex((c) => c.id === active.id);
            const newIndex = newCards.findIndex((c) => c.id === over.id);

            if (oldIndex === -1 || newIndex === -1) return prev;

            // Reorder: Remove from old, insert at new (handles shift direction)
            const [movedCard] = newCards.splice(oldIndex, 1);
            newCards.splice(newIndex, 0, movedCard);

            // Compute midpoint position for the moved card based on new neighbors
            const prevCard = newIndex > 0 ? newCards[newIndex - 1] : null;
            const nextCard = newIndex < newCards.length - 1 ? newCards[newIndex + 1] : null;

            let newPosition: number;
            if (!prevCard && !nextCard)
            {
                // Empty list
                newPosition = 1024.0;
            } else if (!prevCard)
            {
                // Moved to start
                newPosition = nextCard!.position / 2;
            } else if (!nextCard)
            {
                // Moved to end
                newPosition = prevCard.position + 1024.0;
            } else
            {
                // Between two cards
                newPosition = (prevCard.position + nextCard.position) / 2;
            }

            // Update moved card's position
            movedCard.position = newPosition;

            // Sort the array to reflect the new order (though midpoint should fit naturally)
            return newCards.sort((a, b) => a.position - b.position);
        });

        try
        {
            const movedCard = localCards.find((c) => c.id === active.id);
            if (!movedCard)
                return;

            const patchOps = [
                { op: 'replace', path: '/position', value: movedCard.position }
            ];

            if (patchOps.length === 0)
                return;

            await api.patch(
                `/api/boards/${boardId}/lists/${list.id}/cards/${movedCard.id}`,
                patchOps,
                { headers: { 'Content-Type': 'application/json-patch+json' } }
            );

            await refetch();
        }
        catch (error)
        {
            await refetch();
        }
    }

    return (
        <div ref={setNodeRef} style={style} {...attributes} className="bg-gray-100 rounded-lg p-4 flex flex-col w-[300px] min-w-[300px] max-w-[300px] min-h-[220px]">
            <div
                className="flex mb-4 p-2 rounded-t-lg cursor-grab relative"
                {...listeners}
                style={{ backgroundColor: argbToRgba(listColor) }}
            >
                {editingTitle ? (
                    <div className="flex flex-col w-full">
                        <form
                            className="flex items-center w-full"
                            onSubmit={e => { e.preventDefault(); handleTitleSubmit(); }}
                            onPointerDown={e => e.stopPropagation()}
                        >
                            <input
                                className="text-lg font-semibold text-black break-words whitespace-normal line-clamp-2 max-w-full pr-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-400"
                                value={titleInput}
                                onChange={e => setTitleInput(e.target.value)}
                                autoFocus
                                disabled={titleLoading}
                                onBlur={handleInputBlur}
                                onPointerDown={e => e.stopPropagation()}
                            />
                            <button
                                type="button"
                                className="ml-2 text-green-600 hover:text-green-800"
                                onMouseDown={() => { checkmarkClicked.current = true; }}
                                onClick={handleTitleSubmit}
                                disabled={titleLoading}
                                aria-label="Save title"
                                onPointerDown={e => e.stopPropagation()}
                            >
                                {/* Checkmark SVG */}
                                <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                </svg>
                            </button>
                        </form>
                        {titleError && (
                            <div className="text-red-600 text-xl mt-1">{titleError}</div>
                        )}
                    </div>
                ) : (
                    <h3
                        className="text-lg font-semibold text-black break-words whitespace-normal line-clamp-2 max-w-full pr-8 cursor-text"
                        onClick={handleTitleEdit}
                        onPointerDown={e => e.stopPropagation()}
                        title="Click to edit title"
                    >
                        {list.title}
                    </h3>
                )}

                {/* Only show ellipsis when not editing */}
                {!editingTitle && (
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
                                className="absolute -left-1 mt-2 w-52 bg-white rounded-xl shadow-xl z-50 py-2 border border-gray-200"
                                onPointerDown={e => e.stopPropagation()}
                            >
                                {/* Arrow */}
                                <div className="absolute -top-2 left-3 w-4 h-4 z-10">
                                    <div className="w-4 h-4 bg-white rotate-45 shadow-lg border-t border-l border-gray-200" />
                                </div>
                                <button
                                    className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
                                    onClick={() => setShowColorPicker(true)}
                                >
                                    <span className="w-2 h-2 rounded-full" style={{ backgroundColor: argbToRgba(listColor) }}></span>
                                    Change list color
                                </button>
                                {showColorPicker && (
                                    <ListColorPicker
                                        boardId={list.boardId}
                                        listId={list.id}
                                        currentColor={listColor}
                                        title={list.title}
                                        position={list.position}
                                        onColorChanged={handleColorChanged}
                                        onClose={() => setShowColorPicker(false)}
                                    />
                                )}
                                <button
                                    className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
                                >
                                    <span className="w-2 h-2 rounded-full bg-gray-400"></span>
                                    More info
                                </button>
                                <DeleteListButton
                                    listId={list.id}
                                    onDeleted={() => { onDeleted() }}
                                />
                            </div>
                        )}
                    </div>
                )}
            </div>

            <div className='overflow-y-auto overflow-x-hidden max-h-[360px] pr-1 scrollbar-on-hover'>
                <DndContext
                    sensors={sensors}
                    collisionDetection={closestCenter}
                    onDragEnd={handleCardDragEnd}
                >
                    <SortableContext items={localCards.map(c => c.id)} strategy={rectSortingStrategy}>
                        <ul className="space-y-3 flex-1">
                            {localCards.map((card) => (
                                <SortableCard key={card.id} card={card} onDeleted={() => setLocalCards(cards => cards.filter(c => c.id !== card.id))} refetch={refetch} />
                            ))}
                        </ul>
                    </SortableContext>

                    {/* If there are no cards, show AddCardButton at the top */}
                    {addingCard && localCards.length === 0 && (
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
                    )}
                </DndContext>
                {/* If there are cards, show AddCardButton at the bottom when addingCard is true */}
                {addingCard && localCards.length > 0 && (
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
                )}
            </div>

            {!addingCard && (
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