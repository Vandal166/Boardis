import React, { useState, useEffect, useRef } from 'react';

interface Props
{
    open: boolean;
    onClose: () => void;
    onCreate: (data: { title: string }) => Promise<void>;
    loading?: boolean;
    error?: string | null;
    fieldErrors?: { [key: string]: string[] };
}

const CreateListDropdown: React.FC<Props> = ({
    open,
    onClose,
    onCreate,
    loading = false,
    error = null,
    fieldErrors = {},
}) =>
{
    const [title, setTitle] = useState('');
    const panelRef = useRef<HTMLDivElement>(null);

    // Reset title when closed
    useEffect(() =>
    {
        if (!open) setTitle('');
    }, [open]);

    // Close dropdown on outside click
    useEffect(() =>
    {
        if (!open) return;
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
        await onCreate({ title: title.trim() });
    };

    return (
        <div
            ref={panelRef}
            className={`
                absolute left-1/2 -translate-x-1/2 top-8/12 mt-3 w-80 z-10
                bg-white rounded-lg shadow-lg p-6
                transition-all duration-200
                ${open ? 'opacity-100 scale-100 pointer-events-auto' : 'opacity-0 scale-95 pointer-events-none'}
            `}
        >
            {/* Arrow */}
            <div className="absolute -top-2 left-1/2 -translate-x-1/2 w-4 h-4">
                <div className="w-4 h-4 bg-white rotate-45 shadow-lg" />
            </div>
            <h2 className="text-lg font-bold mb-4">Create List</h2>
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">
                        Title <span className="text-red-500">*</span>
                    </label>
                    <input
                        type="text"
                        value={title}
                        onChange={e => setTitle(e.target.value)}
                        className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-400"
                        placeholder='Enter list title'
                        required
                    />
                    {/* Field error for title */}
                    {fieldErrors.Title && fieldErrors.Title.map((err, idx) => (
                        <div key={idx} className="text-red-600 text-sm mt-1">
                            {err}
                        </div>
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
                        Cancel
                    </button>
                    <button
                        type="submit"
                        className="px-4 py-2 rounded bg-blue-600 text-white hover:bg-blue-700 transition"
                        disabled={loading}
                    >
                        {loading ? 'Creating...' : 'Create'}
                    </button>
                </div>
            </form>
        </div>
    );
};

export default CreateListDropdown;