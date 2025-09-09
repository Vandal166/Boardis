import React from 'react';
import { useConfirmation } from './ConfirmationDialog';

interface RemoveBoardMemberButtonProps
{
    memberId: string;
    onRemove: (memberId: string) => void;
    isLoading?: boolean;
}

const RemoveBoardMemberButton: React.FC<RemoveBoardMemberButtonProps> = ({
    memberId,
    onRemove,
    isLoading = false,
}) =>
{

    const confirmation = useConfirmation();

    const handleClick = async () =>
    {
        const confirmed = await confirmation.confirm({
            title: 'Remove Member',
            message: 'Are you sure you want to remove this member from the board?',
            confirmText: 'Remove',
            cancelText: 'Cancel',
        });
        if (confirmed) onRemove(memberId);
    };
    return (
        <button
            className="ml-2 text-red-500 hover:text-red-700 text-sm px-2 py-1 rounded transition"
            onClick={handleClick}
            title="Remove from board"
            disabled={isLoading}
        >
            Remove
        </button>
    );
};

export default RemoveBoardMemberButton;