import React from 'react';
import { useConfirmation } from './ConfirmationDialog';
import api from '../api';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

interface DeleteCardButtonProps
{
    cardId: string;
    listId: string;
    onDeleted: () => void;
}

const DeleteCardButton: React.FC<DeleteCardButtonProps> = ({ cardId, listId, onDeleted }) =>
{
    const confirmation = useConfirmation();
    const { boardId } = useParams<{ boardId: string }>();
    const { t } = useTranslation();

    const handleDelete = async () =>
    {
        const confirmed = await confirmation.confirm({
            title: t('deleteCardTitle'),
            message: t('deleteCardMessage'),
            confirmText: t('deleteCardConfirm'),
            cancelText: t('deleteCardCancel'),
        });
        if (!confirmed) return;
        try
        {
            await api.delete(`/api/boards/${boardId}/lists/${listId}/cards/${cardId}`);
            toast.success(t('deleteCardSuccess'));
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
            {t('deleteCardButton')}
        </button>
    );
};

export default DeleteCardButton;
