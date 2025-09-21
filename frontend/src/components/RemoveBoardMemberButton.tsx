import React from 'react';
import { useConfirmation } from './ConfirmationDialog';
import { useTranslation } from 'react-i18next';

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
    const { t } = useTranslation();

    const handleClick = async () =>
    {
        const confirmed = await confirmation.confirm({
            title: t('removeMemberTitle'),
            message: t('removeMemberMessage'),
            confirmText: t('removeMemberConfirm'),
            cancelText: t('removeMemberCancel'),
        });
        if (confirmed) onRemove(memberId);
    };
    return (
        <button
            className="ml-2 text-red-500 hover:text-red-700 text-sm px-2 py-1 rounded transition"
            onClick={handleClick}
            title={t('removeMemberTooltip')}
            disabled={isLoading}
        >
            {t('removeMemberButton')}
        </button>
    );
};

export default RemoveBoardMemberButton;