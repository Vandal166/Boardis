import React from 'react';
import { useConfirmation } from './ConfirmationDialog';
import api from '../api';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

interface DeleteListButtonProps
{
    listId: string;
    onDeleted: () => void;
}

const DeleteListButton: React.FC<DeleteListButtonProps> = ({ listId, onDeleted }) =>
{
    const confirmation = useConfirmation();
    const { boardId } = useParams<{ boardId: string }>();
    const { t } = useTranslation();

    const handleDelete = async () =>
    {
        const confirmed = await confirmation.confirm({
            title: t('deleteListTitle'),
            message: t('deleteListMessage'),
            confirmText: t('deleteListConfirm'),
            cancelText: t('deleteListCancel'),
        });
        if (!confirmed) return;

        try
        {
            await api.delete(`/api/boards/${boardId}/lists/${listId}`);
            toast.success(t('deleteListSuccess'));
            onDeleted();
        }
        catch { }
    };

    return (
        <button
            className="flex items-center gap-2 w-full text-left px-5 py-2 text-red-600 hover:bg-red-50 hover:border-l-4 hover:border-red-500 transition"
            onClick={handleDelete}
        >
            <span className="w-2 h-2 rounded-full bg-red-400"></span>
            {t('deleteListButton')}
        </button>
    );
};

export default DeleteListButton;