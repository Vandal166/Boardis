import React, { useEffect, useRef, useState } from 'react';
import api from '../api';
import { useConfirmation } from './ConfirmationDialog';

interface BoardSettingsPanelProps
{
    onClose: () => void;
    position?: { top: number; right: number };
    boardId: string;
    title: string;
    description?: string;
    onUpdated?: (updated: { id: string; title: string; description?: string }) => void;
    onDeleted?: () => void;
}

const BoardSettingsPanel: React.FC<BoardSettingsPanelProps> = ({ onClose, position, boardId, title, description, onUpdated, onDeleted }) =>
{
    const [show, setShow] = useState(false);
    const [visible, setVisible] = useState(true); // controls exit animation
    const panelRef = useRef<HTMLDivElement>(null);

    // Editing state
    const [editingField, setEditingField] = useState<'title' | 'description' | null>(null);
    const [editTitle, setEditTitle] = useState(title);
    const [editDescription, setEditDescription] = useState(description || '');
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [deleting, setDeleting] = useState(false);
    const confirm = useConfirmation();

    useEffect(() =>
    {
        setEditTitle(title);
        setEditDescription(description || '');
    }, [title, description]);

    useEffect(() =>
    {
        // Trigger the animation after mount
        setShow(true);
    }, []);

    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                setVisible(false);
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, []);

    // Handle transition end for exit
    useEffect(() =>
    {
        if (!visible)
        {
            const timeout = setTimeout(() =>
            {
                onClose();
            }, 50); // match requested timeout
            return () => clearTimeout(timeout);
        }
    }, [visible, onClose]);

    // When close button is clicked
    const handleRequestClose = () =>
    {
        setVisible(false);
    };

    // Compute style for positioning
    const panelStyle: React.CSSProperties = position
        ? {
            position: 'absolute',
            top: position.top,
            right: position.right,
            willChange: 'opacity, transform',
        }
        : { willChange: 'opacity, transform' };

    // Save handler
    const handleSave = async () =>
    {
        setSaving(true);
        setError(null);
        try
        {
            const patchOps = [
                ...(editTitle !== title ? [{ op: 'replace', path: '/title', value: editTitle }] : []),
                ...(editDescription !== (description || '') ? [{ op: 'replace', path: '/description', value: editDescription }] : []),
            ]
            if (patchOps.length === 0) // if patchOps is empty, nothing to save
            {
                setEditingField(null);
                setSaving(false);
                return;
            }
            await api.patch(`/api/boards/${boardId}`, patchOps, { headers: { 'Content-Type': 'application/json-patch+json' } });
            setEditingField(null);
            // Notify parent of update
            if (onUpdated)
            {
                onUpdated({ id: boardId, title: editTitle, description: editDescription });
            }
        }
        catch (err: any)
        {
            if (err?.response?.status !== 403)
            {
                const errorsObj = err.response?.data?.errors;
                let msg: string | undefined;

                if (errorsObj && typeof errorsObj === 'object')
                {
                    msg = Object.values(errorsObj)
                        .flat()
                        .join(' ');
                }

                if (!msg)
                {
                    msg =
                        err.response?.data?.detail ||
                        err.response?.data?.title ||
                        'Failed to update';
                }

                setError(msg ?? null);
            }
        }
        finally
        {
            setSaving(false);
        }
    };

    // Delete handler
    const handleDeleteBoard = async () =>
    {
        const confirmed = await confirm.confirm({
            title: 'Delete Board',
            message: 'Are you sure you want to delete this board? This action cannot be undone.',
            confirmText: 'Delete',
            cancelText: 'Cancel'
        });
        if (!confirmed)
            return;
        setDeleting(true);
        setError(null);
        try
        {
            const response = await api.delete(`/api/boards/${boardId}`);

            if (response.status === 204)
            {
                if (onDeleted)
                {
                    onDeleted();
                }
                onClose();
            }
        }
        catch (err: any)
        {
            //toast.error(err?.response?.data?.detail || 'Failed to delete board');
            setError('Failed to delete board');
        }
        finally
        {
            setDeleting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50">
            {/* Overlay */}
            <div
                className="absolute inset-0 bg-opacity-0"
                onClick={handleRequestClose}
            />
            {/* Modal */}
            <div
                ref={panelRef}
                className={`
                    absolute top-40 right-2 w-80
                    bg-white rounded-xl shadow-2xl border border-gray-200 p-6
                    transition-all duration-300 ease-out
                    ${show && visible ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={panelStyle}
                onClick={e => e.stopPropagation()}
            >
                {/* Arrow pointing to the gearbox */}
                <div className="absolute -top-2 right-8 w-4 h-4 z-10">
                    <div className="w-4 h-4 bg-white rotate-45 shadow-lg border-t border-l border-gray-200"></div>
                </div>
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">Board Settings</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={handleRequestClose}
                        aria-label="Close settings"
                    >
                        ×
                    </button>
                </div>
                <div className="space-y-4">
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">About this board</h3>
                        {/* Editable Title */}
                        <div className="mb-2">
                            {editingField === 'title' ? (
                                <div className="flex items-center gap-2">
                                    <input
                                        className="border rounded px-2 py-1 w-full text-lg font-semibold"
                                        value={editTitle}
                                        onChange={e => setEditTitle(e.target.value)}
                                        disabled={saving}
                                        autoFocus
                                    />
                                    <button
                                        className="text-green-600 text-xl px-1"
                                        onClick={handleSave}
                                        disabled={saving}
                                        title="Save title"
                                    >
                                        ✓
                                    </button>
                                </div>
                            ) : (
                                <div
                                    className="text-lg font-semibold text-blue-900 cursor-pointer hover:bg-blue-200 hover:rounded px-1 transition"
                                    onClick={() => setEditingField('title')}
                                    title="Click to edit title"
                                >
                                    {editTitle}
                                </div>
                            )}
                        </div>
                        {/* Editable Description */}
                        <div>
                            {editingField === 'description' ? (
                                <div className="flex items-center gap-2">
                                    <textarea
                                        className="border rounded px-2 py-1 w-full text-sm"
                                        value={editDescription}
                                        onChange={e => setEditDescription(e.target.value)}
                                        rows={2}
                                        disabled={saving}
                                        autoFocus
                                    />
                                    <button
                                        className="text-green-600 text-xl px-1"
                                        onClick={handleSave}
                                        disabled={saving}
                                        title="Save description"
                                    >
                                        ✓
                                    </button>
                                </div>
                            ) : (
                                <div
                                    className="text-gray-600 text-sm cursor-pointer italic hover:bg-blue-200 hover:rounded px-1 transition"
                                    onClick={() => setEditingField('description')}
                                    title="Click to edit description"
                                >
                                    {editDescription || <span className="italic text-gray-400">No description</span>}
                                </div>
                            )}
                        </div>
                        {error && <div className="text-red-500 text-xs mt-1">{error}</div>}
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">Visibility</h3>
                        <select className="w-full border rounded px-2 py-1">
                            <option>Private</option>
                            <option>Workspace visible</option>
                            <option>Public</option>
                        </select>
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">Change wallpaper image</h3>
                        <button className="px-3 py-1 bg-gray-100 rounded hover:bg-gray-200 transition text-sm">
                            Change wallpaper
                        </button>
                    </div>
                    <div>
                        <button
                            className="w-full px-3 py-2 bg-red-100 text-red-700 rounded hover:bg-red-200 transition font-semibold"
                            onClick={handleDeleteBoard}
                            disabled={deleting}
                        >
                            {deleting ? 'Deleting...' : 'Delete board'}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BoardSettingsPanel;