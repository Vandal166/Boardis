import { useSortable } from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";
import { useState, useRef, useEffect } from "react";
import api from '../api';
import { EllipsisHorizontalIcon } from '@heroicons/react/24/solid';
import { createPortal } from 'react-dom';
import DeleteCardButton from './DeleteCardButton';
import { useConfirmationDialogOpen } from './ConfirmationDialog';

type BoardCard = {
    id: string;
    boardId: string;
    boardListId: string;
    title: string;
    description?: string;
    position: number;
    completedAt?: string; // ISO string representation of DateTime from .NET
};

function SortableCard({ card, onDeleted, refetch }: { card: BoardCard, onDeleted: () => void, refetch: () => void })
{
    const { attributes, listeners, setNodeRef, transform, transition, isDragging } =
        useSortable({ id: card.id });

    const [editing, setEditing] = useState(false);
    const [titleInput, setTitleInput] = useState(card.title);
    const [title, setTitle] = useState(card.title);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [showMenu, setShowMenu] = useState(false);
    const [menuPos, setMenuPos] = useState<{ top: number; left: number } | null>(null);
    const [completedAt, setCompletedAt] = useState(card.completedAt);
    const checkmarkClicked = useRef(false);
    const menuRef = useRef<HTMLDivElement>(null);
    const menuButtonRef = useRef<HTMLButtonElement>(null);
    const confirmationDialogOpen = useConfirmationDialogOpen();

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

    useEffect(() =>
    {
        if (showMenu && menuButtonRef.current)
        {
            const rect = menuButtonRef.current.getBoundingClientRect();
            setMenuPos({
                top: rect.bottom + window.scrollY + 6,
                left: rect.left + window.scrollX - 8, // shift to left by 8px
            });
        }
    }, [showMenu]);

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.7 : 1,
    };


    const handleSubmit = async () =>
    {
        checkmarkClicked.current = false;
        if (titleInput === title)
        {
            setEditing(false);
            setError(null);
            return;
        }
        setLoading(true);
        setError(null);
        try
        {
            await api.put(`/api/boards/${card.boardId}/lists/${card.boardListId}/cards/${card.id}`, {
                title: titleInput,
                description: card.description ?? '',
                position: card.position,
                completedAt: card.completedAt ?? null,
            });
            setTitle(titleInput); // update local title state
            setEditing(false);
            setError(null);
        } catch (err: any)
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
            setError(message);
        } finally
        {
            setLoading(false);
        }
    };

    const handleInputBlur = () =>
    {
        if (checkmarkClicked.current)
        {
            checkmarkClicked.current = false;
            return;
        }
        setEditing(false);
    };

    const handleMenuToggle = (e: React.MouseEvent) =>
    {
        e.stopPropagation();
        setShowMenu(v => !v);
    };

    const handleComplete = async () =>
    {
        const newCompletedAt = completedAt ? undefined : new Date().toISOString();
        setLoading(true);
        try
        {
            await api.patch(`/api/boards/${card.boardId}/lists/${card.boardListId}/cards/${card.id}`, {
                completedAt: newCompletedAt ?? null,
            });
            setCompletedAt(newCompletedAt);
            if (refetch) refetch();
        } catch
        {
            // Optionally show error
        } finally
        {
            setLoading(false);
        }
    };

    return (
        <li
            ref={setNodeRef}
            style={style}
            className="bg-white p-3 rounded shadow-md cursor-grab relative"
        >
            <div
                className="flex items-center gap-2 mb-1 cursor-grab relative"
                {...attributes}
                {...listeners}
            >
                {/* Checkbox */}
                <button
                    className={`flex items-center justify-center border-2 transition-colors mr-2 focus:outline-none rounded-full ${completedAt ? 'bg-green-400 border-green-500' : 'bg-gray-300 border-gray-400'}`}
                    aria-label={completedAt ? 'Mark as not completed' : 'Mark as completed'}
                    onClick={handleComplete}
                    disabled={loading}
                    tabIndex={0}
                    style={{ width: 22, height: 22, minWidth: 22, minHeight: 22, aspectRatio: '1 / 1' }}
                >
                    {completedAt && (
                        <svg className="w-3 h-3 text-white" fill="none" stroke="currentColor" strokeWidth={3} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M5 13l4 4L19 7" />
                        </svg>
                    )}
                </button>
                {/* Card title or input */}
                {editing ? (
                    <div className="flex flex-col w-full">
                        <form
                            className="flex items-center w-full"
                            onSubmit={e => { e.preventDefault(); handleSubmit(); }}
                            onPointerDown={e => e.stopPropagation()}
                        >
                            <input
                                className="flex-grow text-base font-medium text-blue-800 break-words whitespace-normal line-clamp-2 max-w-full pr-2 rounded border border-gray-300 focus:outline-none focus:ring-2 focus:ring-blue-400"
                                value={titleInput}
                                onChange={e => setTitleInput(e.target.value)}
                                autoFocus
                                disabled={loading}
                                onBlur={handleInputBlur}
                                onPointerDown={e => e.stopPropagation()}
                            />
                            <div className="absolute top-0 right-1">
                                <button
                                    type="button"
                                    className="ml-2 hover:text-green-800 flex-shrink-0"
                                    onMouseDown={() => { checkmarkClicked.current = true; }}
                                    onClick={handleSubmit}
                                    disabled={loading}
                                    aria-label="Save title"
                                    onPointerDown={e => e.stopPropagation()}
                                >
                                    {/* Checkmark SVG */}
                                    <svg xmlns="http://www.w3.org/2000/svg" className="h-6 w-6 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 13l4 4L19 7" />
                                    </svg>
                                </button>
                            </div>
                        </form>
                        {error && (
                            <div className="text-red-600 text-sm mt-1">{error}</div>
                        )}
                    </div>
                ) : (
                    <h4
                        className="font-medium text-blue-800 cursor-text select-none inline-block"
                        onClick={e =>
                        {
                            e.stopPropagation();
                            setEditing(true);
                            setTitleInput(title);
                            setError(null);
                        }}
                        title="Click to edit title"
                    >
                        {title}
                    </h4>
                )}
                {/* Ellipsis menu */}
                {!editing && (
                    <div className="absolute top-1 right-1">
                        <button
                            ref={menuButtonRef}
                            aria-label="Card settings"
                            onPointerDown={e => e.stopPropagation()}
                            onClick={handleMenuToggle}
                            className="rounded hover:bg-gray-100 transition"
                        >
                            <EllipsisHorizontalIcon className="w-5 h-5 text-gray-500" />
                        </button>
                        {showMenu && menuPos && createPortal(
                            <div
                                ref={menuRef}
                                className="absolute w-52 bg-white rounded-xl shadow-xl z-[50] py-2 border border-gray-200"
                                style={{ top: menuPos.top, left: menuPos.left, position: 'absolute' }}
                                onPointerDown={e => e.stopPropagation()}
                            >
                                {/* Arrow */}
                                <div className="absolute -top-2 left-3 w-4 h-4 z-10">
                                    <div className="w-4 h-4 bg-white rotate-45 shadow-lg border-t border-l border-gray-200" />
                                </div>
                                <button
                                    className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
                                    onClick={() => { setShowMenu(false); /* TODO: Show details modal */ }}
                                >
                                    <span className="w-2 h-2 rounded-full bg-blue-400"></span>
                                    Details
                                </button>
                                <button
                                    className="flex items-center gap-2 w-full text-left px-5 py-2 text-gray-800 hover:bg-blue-50 transition"
                                    onClick={() => { setShowMenu(false); handleComplete(); }}
                                >
                                    <span className="w-2 h-2 rounded-full bg-green-400"></span>
                                    Mark as completed
                                </button>
                                <DeleteCardButton cardId={card.id} listId={card.boardListId} onDeleted={onDeleted} />
                            </div>,
                            document.body
                        )}
                    </div>
                )}
            </div>
            {card.description && (
                <p className="text-sm text-gray-600 mt-1">{card.description}</p>
            )}
            {completedAt && (
                <p className="text-xs text-green-600 mt-1">
                    Completed: {new Date(completedAt).toLocaleString()}
                </p>
            )}
        </li>
    );
}

export default SortableCard;