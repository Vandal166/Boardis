import React, { useState, useEffect, useRef } from 'react';
import { useTranslation } from 'react-i18next';

interface Props
{
  open: boolean;
  onClose: () => void;
  onCreate: (data: { title: string; description?: string; }) => Promise<void>;
  loading?: boolean;
  error?: string | null;
  fieldErrors?: { [key: string]: string[] };
}

const CreateBoardDropdown: React.FC<Props> = ({
  open,
  onClose,
  onCreate,
  loading = false,
  error = null,
  fieldErrors = {},
}) =>
{
  const [title, setTitle] = useState('');
  const [description, setDescription] = useState('');
  const panelRef = useRef<HTMLDivElement>(null);
  const { t } = useTranslation();

  // Close dropdown on outside click
  useEffect(() =>
  {
    if (!open)
    {
      setTitle('');
      setDescription('');
    }

    function handleClick(e: MouseEvent)
    {
      if (panelRef.current && !panelRef.current.contains(e.target as Node))
      {
        onClose();
      }
    }

    document.addEventListener('mousedown', handleClick);

    return () => document.removeEventListener('mousedown', handleClick);
  }, [open, onClose]);

  const handleSubmit = async (e: React.FormEvent) =>
  {
    e.preventDefault();
    await onCreate({
      title: title.trim(),
      description: description.trim(),
    });
  };

  return (
    <div
      ref={panelRef}
      className={`
      absolute right-0 mt-3 w-80 z-50
      bg-white rounded-lg shadow-lg p-6
      transition-all duration-200
      ${open ? 'opacity-100 scale-100 pointer-events-auto' : 'opacity-0 scale-95 pointer-events-none'}
    `}
      style={{ top: '100%' }}
    >
      {/* Arrow */}
      <div className="absolute -top-2 right-6 w-4 h-4">
        <div className="w-4 h-4 bg-white rotate-45 shadow-lg" />
      </div>
      <h2 className="text-lg font-bold mb-4">{t('createBoardTitle')}</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium mb-1">
            {t('createBoardLabelTitle')} <span className="text-red-500">*</span>
          </label>
          <input
            type="text"
            value={title}
            onChange={e => setTitle(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
            required
          />
          {fieldErrors.Title && fieldErrors.Title.map((msg, i) => (
            <div key={i} className="text-red-600 text-sm mt-1">{msg}</div>
          ))}
        </div>
        <div>
          <label className="block text-sm font-medium mb-1">
            {t('createBoardLabelDescription')}
          </label>
          <textarea
            value={description}
            onChange={e => setDescription(e.target.value)}
            onInput={e =>
            {
              const target = e.target as HTMLTextAreaElement;
              target.style.height = 'auto'; // Reset height
              target.style.height = `${target.scrollHeight}px`; // Set to scrollHeight
            }}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400 overflow-hidden resize-none"
            rows={2}
          />
          {fieldErrors.Description && fieldErrors.Description.map((msg, i) => (
            <div key={i} className="text-red-600 text-sm mt-1">{msg}</div>
          ))}
        </div>
        {error && (
          <div className="text-red-600 text-sm">{error}</div>
        )}
        <div className="flex justify-end gap-2">
          <button
            type="button"
            className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 transition"
            onClick={onClose}
            disabled={loading}
          >
            {t('createBoardCancel')}
          </button>
          <button
            type="submit"
            className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 transition"
            disabled={loading}
          >
            {loading ? t('createBoardCreating') : t('createBoardCreate')}
          </button>
        </div>
      </form>
    </div>
  );
};

export default CreateBoardDropdown;