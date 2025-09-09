import React from 'react';
import { useConfirmation } from './ConfirmationDialog';
import api from '../api';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';

interface DeleteListButtonProps
{
    listId: string;
    onDeleted: () => void;
}

const DeleteListButton: React.FC<DeleteListButtonProps> = ({ listId, onDeleted }) =>
{
    const confirmation = useConfirmation();
    const { boardId } = useParams<{ boardId: string }>();

    const handleDelete = async () =>
    {
        const confirmed = await confirmation.confirm({
            title: 'Delete List',
            message: 'Are you sure you want to delete this list? All cards in this list will also be deleted.',
            confirmText: 'Delete',
            cancelText: 'Cancel',
        });
        if (!confirmed) return;

        try
        {
            await api.delete(`/api/boards/${boardId}/lists/${listId}`);
            toast.success('List deleted');
            onDeleted();
        } catch (err: any)
        {
            toast.error('Failed to delete list');
        }
    };

    return (
        <button
            className="flex items-center gap-2 w-full text-left px-5 py-2 text-red-600 hover:bg-red-50 hover:border-l-4 hover:border-red-500 transition"
            onClick={handleDelete}
        >
            <span className="w-2 h-2 rounded-full bg-red-400"></span>
            Delete list
        </button>
    );
};

export default DeleteListButton;