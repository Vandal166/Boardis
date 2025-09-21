import { useState } from 'react';
import CreateListDropdown from './CreateListDropdown';
import { useTranslation } from 'react-i18next';

interface AddListButtonProps
{
    onCreate: (data: { title: string }) => Promise<boolean>;
    error?: string | null;
    fieldErrors?: { [key: string]: string[] };
    clearErrors: () => void;
}

const AddListButton: React.FC<AddListButtonProps> = ({
    onCreate,
    error,
    fieldErrors,
    clearErrors,
}) =>
{
    const [showListModal, setShowListModal] = useState(false);
    const { t } = useTranslation();

    return (
        <div className="flex flex-col items-center justify-center w-[300px] min-w-[300px] max-w-[300px] relative">
            <button
                className="bg-blue-600 text-white rounded-full w-10 h-10 flex items-center justify-center shadow transition-transform duration-200 hover:scale-110 hover:bg-blue-700"
                aria-label={t('addListButtonAria')}
                onClick={() => setShowListModal(true)}
            >
                +
            </button>
            <span className="mt-2 text-sm text-gray-600 transition-opacity duration-200">
                {t('addListButtonText')}
            </span>
            <CreateListDropdown
                open={showListModal}
                onClose={() =>
                {
                    setShowListModal(false);
                    clearErrors();
                }}
                onCreate={async (data) =>
                {
                    const success = await onCreate(data);
                    if (success) setShowListModal(false);
                }}
                error={error}
                fieldErrors={fieldErrors}
            />
        </div>
    );
};

export default AddListButton;