import React, { useEffect, useRef, useState } from 'react';
import Spinner from './Spinner';
import AddBoardMemberButton from './AddBoardMemberButton';
import RemoveBoardMemberButton from './RemoveBoardMemberButton';
import { useConfirmationDialogOpen } from './ConfirmationDialog';

interface Member
{
    userId: string;
    username: string;
    email: string;
    role: string;
}

interface Role
{
    key: string;
    displayName: string;
}

interface ManageBoardMembersModalProps
{
    onClose: () => void;
    members: Member[];
    onAdd: (emailOrUsername: string, role: string) => void;
    onRemove: (memberId: string) => void;
    roles: Role[];
    isLoading?: boolean;
}


const ManageBoardMembersModal: React.FC<ManageBoardMembersModalProps> = ({
    onClose,
    members,
    onAdd,
    onRemove,
    roles,
    isLoading = false,
}) =>
{
    const [show, setShow] = useState(false);
    const [error] = useState('');
    const panelRef = useRef<HTMLDivElement>(null);
    const confirmationDialogOpen = useConfirmationDialogOpen();

    useEffect(() => setShow(true), []);

    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (confirmationDialogOpen) return; // Prevent closing if dialog is open
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                onClose();
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [onClose, confirmationDialogOpen]);

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
                    ${show ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={{ willChange: 'opacity, transform' }}
            >
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">Manage Members</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={onClose}
                        aria-label="Close"
                    >
                        ×
                    </button>
                </div>
                <AddBoardMemberButton
                    onAdd={onAdd}
                    roles={roles}
                    isLoading={isLoading}
                />
                {error && <div className="text-red-600 text-sm mb-2">{error}</div>}
                <div>
                    <h3 className="font-semibold mb-2">Current Members</h3>
                    {isLoading ? (
                        <div className="flex justify-center py-8">
                            <Spinner />
                        </div>
                    ) : (
                        <ul className="space-y-2 max-h-90 overflow-y-auto">
                            {members.map(member => (
                                console.log(member),
                                <li key={member.userId} className="flex items-center gap-3 p-2 rounded hover:bg-gray-50">
                                    {/* Avatar icon (simple circle with initials) */}
                                    <div className="w-9 h-9 rounded-full bg-blue-200 flex items-center justify-center text-blue-800 font-bold text-lg">
                                        {member.username.slice(0, 2).toUpperCase()}
                                    </div>
                                    <div className="flex-1">
                                        <div className="flex items-center gap-2">
                                            <span className="font-medium">{member.username}</span>
                                            <span className="text-xs bg-gray-200 px-2 py-0.5 rounded">{member.role}</span>
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
                                <li className="text-gray-400 text-sm">No members yet.</li>
                            )}
                        </ul>
                    )}
                </div>
            </div>
        </div>
    );
};

export default ManageBoardMembersModal;