import { useTranslation } from 'react-i18next';

interface AddCardButtonProps
{
    value: string;
    error?: string | null;
    fieldErrors?: { [key: string]: string[] };
    onChange: (value: string) => void;
    onAdd: (e: React.FormEvent) => void;
    onCancel: () => void;
}

function AddCardButton({ value, error, fieldErrors, onChange, onAdd, onCancel }: AddCardButtonProps)
{
    const { t } = useTranslation();

    return (
        <form onSubmit={onAdd} className="mt-4 flex flex-col gap-2">
            <input
                autoFocus
                type="text"
                value={value}
                onChange={e => onChange(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
                placeholder={t('addCardInputPlaceholder')}
            />
            {/* Field error for title */}
            {fieldErrors?.Title && fieldErrors.Title.map((err, idx) => (
                <div key={idx} className="text-red-600 text-sm">{err}</div>
            ))}
            {error && <div className="text-red-600 text-sm">{error}</div>}
            <div className="flex gap-2">
                <button
                    type="submit"
                    className="px-3 py-1 rounded bg-blue-600 text-white hover:bg-blue-700 transition"
                >
                    {t('addCardButton')}
                </button>
                <button
                    type="button"
                    className="px-3 py-1 rounded bg-gray-200 hover:bg-gray-300 transition"
                    onClick={onCancel}
                >
                    {t('addCardCancel')}
                </button>
            </div>
        </form>
    );
}

export default AddCardButton;