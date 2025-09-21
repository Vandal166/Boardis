import React, { useEffect, useRef, useState } from 'react';
import Spinner from './Spinner';
import AddBoardMemberButton from './AddBoardMemberButton';
import RemoveBoardMemberButton from './RemoveBoardMemberButton';
import { useConfirmationDialogOpen } from './ConfirmationDialog';
import api from '../api';
import { toast } from 'react-hot-toast';
import { useBoardSignalR } from '../communication/BoardSignalRProvider';
import { useTranslation } from 'react-i18next';

interface Member
{
    userId: string;
    username: string;
    email: string;
    permissions?: string[];
}

interface ManageBoardMembersModalProps
{
    onClose: () => void;
    boardId: string;
    members: Member[];
    onAdd: (emailOrUsername: string) => void;
    onRemove: (memberId: string) => void;
    isLoading?: boolean;
    fetchMembers?: () => void;
}

const ManageBoardMembersModal: React.FC<ManageBoardMembersModalProps> = ({
    onClose,
    boardId,
    members,
    onAdd,
    onRemove,
    isLoading = false,
    fetchMembers,
}) =>
{
    const [show, setShow] = useState(false);
    const [visible, setVisible] = useState(true); // controls exit animation
    const [error] = useState('');
    const panelRef = useRef<HTMLDivElement>(null);
    const listRef = useRef<HTMLUListElement>(null);
    const confirmationDialogOpen = useConfirmationDialogOpen();
    const boardHubConnection = useBoardSignalR();
    const { t } = useTranslation();

    // permissions popover state
    const [permState, setPermState] = useState<{
        memberId: string;
        loading: boolean;
        permissions: string[];
        all: string[];
        adding?: string;
        removing?: string;
        error?: string;
        pos?: { top: number; left: number }; // position inside modal
    } | null>(null);

    useEffect(() => setShow(true), []);

    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (confirmationDialogOpen) return; // Prevent closing if dialog is open
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                setVisible(false);
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [confirmationDialogOpen]);

    // Handle transition end for exit
    useEffect(() =>
    {
        if (!visible)
        {
            const timeout = setTimeout(() =>
            {
                onClose();
            }, 50);
            return () => clearTimeout(timeout);
        }
    }, [visible, onClose]);

    useEffect(() =>
    {
        function handleMemberAdded(updatedBoardId: string)
        {
            if (updatedBoardId === boardId && fetchMembers)
            {
                fetchMembers();
            }
        }
        function handleMemberRemoved(updatedBoardId: string)
        {
            if (updatedBoardId === boardId && fetchMembers)
            {
                fetchMembers();
            }
        }

        function handleBoardMemberLeft(updatedBoardId: string)
        {
            if (updatedBoardId === boardId && fetchMembers)
            {
                fetchMembers();
                toast.success('A member has left the board.');
            }
        }
        boardHubConnection.on('BoardMemberAdded', handleMemberAdded);
        boardHubConnection.on('BoardMemberRemoved', handleMemberRemoved);
        boardHubConnection.on('BoardMemberLeft', handleBoardMemberLeft);

        return () =>
        {
            boardHubConnection.off('BoardMemberAdded', handleMemberAdded);
            boardHubConnection.off('BoardMemberRemoved', handleMemberRemoved);
            boardHubConnection.off('BoardMemberLeft', handleBoardMemberLeft);
        };
    }, [boardHubConnection, boardId, fetchMembers]);

    const fetchPermissions = async (memberId: string) =>
    {
        setPermState(s => s
            ? { ...s, memberId, loading: true, permissions: [], all: [], error: undefined }
            : { memberId, loading: true, permissions: [], all: [], pos: { top: 0, left: 0 } }
        );
        try
        {
            const [userRes, allRes] = await Promise.all([
                api.get(`/api/boards/${boardId}/members/${memberId}/permissions`),
                api.get('/api/permissions')
            ]);
            const userPerms = Array.isArray(userRes.data)
                ? userRes.data
                : (userRes.data.permissions ?? userRes.data.Permissions ?? []);
            let allPerms = Array.isArray(allRes.data)
                ? allRes.data
                : (allRes.data.permissions ?? allRes.data.Permissions ?? []);
            allPerms = allPerms.filter((perm: string) => perm !== 'None');
            setPermState(s => s ? { ...s, loading: false, permissions: userPerms, all: allPerms } : s);
        }
        catch
        {
            setPermState(s => s ? { ...s, loading: false, permissions: [], all: [], error: 'Failed to load permissions.' } : s);
        }
    };

    const addPermission = async (memberId: string, permission: string) =>
    {
        if (!permState || permState.adding || permState.removing) return;
        setPermState(s => s && { ...s, adding: permission });
        try
        {
            await api.put(`/api/boards/${boardId}/members/${memberId}/permissions`, { permission });
            setPermState(s =>
                s
                    ? {
                        ...s,
                        adding: undefined,
                        permissions: s.permissions.includes(permission)
                            ? s.permissions
                            : [...s.permissions, permission]
                    }
                    : s
            );
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
            setPermState(s => s && { ...s, adding: undefined });
        }
    }

    const removePermission = async (memberId: string, permission: string) =>
    {
        if (!permState || permState.adding || permState.removing) return;
        setPermState(s => s && { ...s, removing: permission });
        try
        {
            await api.delete(`/api/boards/${boardId}/members/${memberId}/permissions`, { data: { permission } });
            setPermState(s =>
                s
                    ? {
                        ...s,
                        removing: undefined,
                        permissions: s.permissions.filter(p => p !== permission)
                    }
                    : s
            );
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
            setPermState(s => s && { ...s, removing: undefined });
        }
    };

    const togglePermissions = (memberId: string, anchorEl?: HTMLElement) =>
    {
        if (permState?.memberId === memberId)
        {
            setPermState(null);
            return;
        }
        if (!anchorEl || !panelRef.current)
        {
            setPermState(null);
            return;
        }
        const aRect = anchorEl.getBoundingClientRect();
        const rootRect = panelRef.current.getBoundingClientRect();
        const width = 288; // w-72
        let left = aRect.left - rootRect.left;
        if (left + width > rootRect.width) left = Math.max(8, rootRect.width - width - 8);
        const top = aRect.bottom - rootRect.top + 4;
        setPermState({
            memberId,
            loading: true,
            permissions: [],
            all: [],
            pos: { top, left }
        });
        void fetchPermissions(memberId);
    };

    const handleAddWrapper = (emailOrUsername: string) =>
    {
        onAdd(emailOrUsername);
    };

    // Close permission panel if list scrolls (simpler than re-positioning)
    const handleMembersScroll = () =>
    {
        if (permState) setPermState(null);
    };

    // When close button is clicked
    const handleRequestClose = () =>
    {
        setVisible(false);
    };

    return (
        <div className="fixed inset-0 z-50">
            {/* Overlay */}
            <div className="absolute inset-0 bg-black opacity-40" />
            {/* Modal */}
            <div
                ref={panelRef}
                className={`
                    absolute top-40 left-1/2 -translate-x-1/2 w-full max-w-2xl
                    bg-white rounded-xl shadow-2xl border border-gray-200 p-6
                    transition-all duration-300 ease-out
                    ${show && visible ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={{ willChange: 'opacity, transform' }}
            >
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">{t('manageMembersTitle')}</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={handleRequestClose}
                        aria-label={t('manageMembersCloseAria')}
                    >
                        ×
                    </button>
                </div>
                <AddBoardMemberButton
                    onAdd={handleAddWrapper}
                    isLoading={isLoading}
                />
                {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
                <div>
                    <h3 className="font-semibold mb-2">{t('manageMembersCurrentMembers')}</h3>
                    {isLoading ? (
                        <div className="flex justify-center py-8">
                            <Spinner />
                        </div>
                    ) : (
                        <ul
                            ref={listRef}
                            onScroll={handleMembersScroll}
                            className="space-y-2 max-h-90 overflow-y-auto"
                        >
                            {members.map(member => (
                                <li
                                    key={member.userId}
                                    className="relative flex items-center gap-3 p-2 rounded hover:bg-gray-50"
                                >
                                    {/* Avatar icon (simple circle with initials) */}
                                    <div className="w-9 h-9 rounded-full bg-blue-200 flex items-center justify-center text-blue-800 font-bold text-lg">
                                        {member.username.slice(0, 2).toUpperCase()}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                            <span className="font-medium">{member.username}</span>
                                            <button
                                                onClick={(e) => togglePermissions(member.userId, e.currentTarget)}
                                                className="text-xs bg-gray-200 px-2 py-0.5 rounded hover:bg-gray-300 transition"
                                                aria-label={t('manageMembersViewPermissionsAria')}
                                            >
                                                {t('manageMembersPermissions')}
                                            </button>
                                        </div>
                                        <div className="text-xs text-gray-500">{member.email}</div>
                                    </div>

                                    <RemoveBoardMemberButton
                                        memberId={member.userId}
                                        onRemove={onRemove}
                                        isLoading={isLoading}
                                    />
                                </li>
                            ))}
                            {members.length === 0 && (
                                <li className="text-gray-400 text-sm">{t('manageMembersNoMembers')}</li>
                            )}
                        </ul>
                    )}
                </div>

                {permState && permState.pos && (
                    <div
                        className="absolute z-50 w-72 bg-white border border-gray-200 rounded shadow-lg p-3"
                        style={{ top: permState.pos.top, left: permState.pos.left }}
                    >
                        <div className="flex justify-between items-center mb-2">
                            <span className="text-xs font-semibold uppercase tracking-wide text-gray-600">
                                {t('manageMembersPermissions')}
                            </span>
                            <button
                                className="text-xs text-gray-400 hover:text-gray-600"
                                onClick={() => setPermState(null)}
                                aria-label={t('manageMembersClosePermissionsPanelAria')}
                            >
                                ×
                            </button>
                        </div>
                        {permState.loading && (
                            <div className="flex justify-center py-2">
                                <Spinner />
                            </div>
                        )}
                        {!permState.loading && permState.error && (
                            <div className="text-xs text-red-600">{t('manageMembersErrorLoadingPermissions')}</div>
                        )}
                        {!permState.loading && !permState.error && (
                            <div className="flex flex-wrap gap-1">
                                {permState.all.length === 0 && (
                                    <span className="text-xs text-gray-400">{t('manageMembersNoPermissionsDefined')}</span>
                                )}
                                {permState.all
                                    .slice()
                                    .sort((a, b) => a.localeCompare(b))
                                    .map(p =>
                                    {
                                        const granted = permState.permissions.includes(p);
                                        const busy = permState.adding === p || permState.removing === p;
                                        return (
                                            <button
                                                key={p}
                                                disabled={busy}
                                                onClick={() =>
                                                    granted
                                                        ? removePermission(permState.memberId, p)
                                                        : addPermission(permState.memberId, p)
                                                }
                                                className={
                                                    'text-[10px] px-2 py-0.5 rounded border transition ' +
                                                    (granted
                                                        ? 'bg-blue-100 text-blue-700 border-blue-200 hover:bg-red-100 hover:text-red-600 hover:border-red-300'
                                                        : 'bg-gray-100 text-gray-400 border-gray-200 hover:bg-gray-200 hover:text-gray-600')
                                                }
                                                title={granted ? t('manageMembersClickToRevoke') : t('manageMembersClickToGrant')}
                                            >
                                                {busy ? '...' : p}
                                            </button>
                                        );
                                    })}
                            </div>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
};

export default ManageBoardMembersModal;