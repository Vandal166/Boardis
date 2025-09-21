import React, { useEffect, useRef, useState } from 'react';
import api from '../api';
import { useConfirmation } from './ConfirmationDialog';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';

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
    const [visible, setVisible] = useState(true);
    const panelRef = useRef<HTMLDivElement>(null);
    const fileInputRef = useRef<HTMLInputElement>(null); // for wallpaper upload

    // Editing state
    const [editingField, setEditingField] = useState<'title' | 'description' | null>(null);
    const [editTitle, setEditTitle] = useState(title);
    const [editDescription, setEditDescription] = useState(description || '');
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [deleting, setDeleting] = useState(false);
    const [uploading, setUploading] = useState(false); // uploading state
    const confirm = useConfirmation();
    const { t } = useTranslation();

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
                        t('boardSettingsFailedToUpdate');
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
            title: t('boardSettingsDeleteBoardConfirmTitle'),
            message: t('boardSettingsDeleteBoardConfirmMessage'),
            confirmText: t('boardSettingsDeleteBoardConfirm'),
            cancelText: t('boardSettingsDeleteBoardCancel')
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
            setError(t('boardSettingsDeleteBoardFailed'));
        }
        finally
        {
            setDeleting(false);
        }
    };

    // Trigger hidden file input
    const handleChangeWallpaperClick = () =>
    {
        fileInputRef.current?.click();
    };

    // Handle file selection + confirm + upload
    const handleWallpaperSelected = async (e: React.ChangeEvent<HTMLInputElement>) =>
    {
        const file = e.target.files?.[0];
        if (!file)
            return;

        const confirmed = await confirm.confirm({
            title: t('boardSettingsChangeWallpaperConfirmTitle'),
            message: t('boardSettingsChangeWallpaperConfirmMessage', { fileName: file.name }),
            confirmText: t('boardSettingsChangeWallpaperConfirmOk'),
            cancelText: t('boardSettingsChangeWallpaperConfirmCancel')
        });
        if (!confirmed)
        {
            e.target.value = '';
            return;
        }

        setUploading(true);
        setError(null);
        try
        {
            const form = new FormData();
            form.append('File', file);

            await api.post(`/api/boards/${boardId}/media`, form, {
                headers: { 'Content-Type': 'multipart/form-data' }
            });
            // Optionally notify parent of update
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
                let msg: string;

                if (errorsObj && typeof errorsObj === 'object')
                {
                    msg = Object.values(errorsObj)
                        .flat()
                        .join(' ');
                }
                else
                {
                    msg =
                        err.response?.data?.detail ||
                        err.response?.data?.title ||
                        t('boardSettingsFailedToUploadImage');
                }

                toast.error(msg);
            }
        }
        finally
        {
            setUploading(false);
            e.target.value = '';
        }
    };

    // Handle wallpaper delete
    const handleDeleteWallpaper = async () =>
    {
        const confirmed = await confirm.confirm({
            title: t('boardSettingsDeleteWallpaperConfirmTitle'),
            message: t('boardSettingsDeleteWallpaperConfirmMessage'),
            confirmText: t('boardSettingsDeleteWallpaperConfirmDelete'),
            cancelText: t('boardSettingsDeleteWallpaperConfirmCancel')
        });
        if (!confirmed) return;

        setUploading(true);
        setError(null);
        try
        {
            await api.delete(`/api/boards/${boardId}/media`);
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
                let msg: string;

                if (errorsObj && typeof errorsObj === 'object')
                {
                    msg = Object.values(errorsObj)
                        .flat()
                        .join(' ');
                }
                else
                {
                    msg =
                        err.response?.data?.detail ||
                        err.response?.data?.title ||
                        t('boardSettingsFailedToDeleteWallpaper');
                }

                toast.error(msg);
            }
        }
        finally
        {
            setUploading(false);
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
                    <h2 className="text-xl font-bold">{t('boardSettingsTitle')}</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={handleRequestClose}
                        aria-label={t('boardSettingsCloseAria')}
                    >
                        ×
                    </button>
                </div>
                <div className="space-y-4">
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">{t('boardSettingsAboutTitle')}</h3>
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
                                        title={t('boardSettingsSaveTitle')}
                                    >
                                        ✓
                                    </button>
                                </div>
                            ) : (
                                <div
                                    className="text-lg font-semibold text-blue-900 cursor-pointer hover:bg-blue-200 hover:rounded px-1 transition"
                                    onClick={() => setEditingField('title')}
                                    title={t('boardSettingsEditTitleAria')}
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
                                        title={t('boardSettingsSaveDescription')}
                                    >
                                        ✓
                                    </button>
                                </div>
                            ) : (
                                <div
                                    className="text-gray-600 text-sm cursor-pointer italic hover:bg-blue-200 hover:rounded px-1 transition"
                                    onClick={() => setEditingField('description')}
                                    title={t('boardSettingsEditDescriptionAria')}
                                >
                                    {editDescription || <span className="italic text-gray-400">{t('boardSettingsNoDescription')}</span>}
                                </div>
                            )}
                        </div>
                        {error && <div className="text-red-500 text-xs mt-1">{error}</div>}
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">{t('boardSettingsVisibilityTitle')}</h3>
                        <select className="w-full border rounded px-2 py-1">
                            <option>{t('boardSettingsVisibilityPrivate')}</option>
                            <option>{t('boardSettingsVisibilityWorkspace')}</option>
                            <option>{t('boardSettingsVisibilityPublic')}</option>
                        </select>
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">{t('boardSettingsWallpaperTitle')}</h3>
                        <div className="flex gap-2">
                            <button
                                className="px-2 py-1 bg-gray-100 rounded hover:bg-gray-200 transition text-sm"
                                onClick={handleChangeWallpaperClick}
                                disabled={uploading}
                            >
                                {uploading ? t('boardSettingsUploading') : t('boardSettingsChangeWallpaper')}
                            </button>
                            <button
                                className="px-2 py-1 bg-red-100 text-red-700 rounded hover:bg-red-200 transition text-sm"
                                onClick={handleDeleteWallpaper}
                                disabled={uploading}
                            >
                                {uploading ? t('boardSettingsDeleting') : t('boardSettingsDeleteWallpaper')}
                            </button>
                        </div>
                        <input
                            ref={fileInputRef}
                            type="file"
                            accept="image/*"
                            className="hidden"
                            onChange={handleWallpaperSelected}
                        />
                    </div>
                    <div>
                        <button
                            className="w-full px-3 py-2 bg-red-100 text-red-700 rounded hover:bg-red-200 transition font-semibold"
                            onClick={handleDeleteBoard}
                            disabled={deleting}
                        >
                            {deleting ? t('boardSettingsDeleteBoardDeleting') : t('boardSettingsDeleteBoard')}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BoardSettingsPanel;