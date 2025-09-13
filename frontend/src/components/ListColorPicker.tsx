import { useState } from 'react';
import api from '../api';
import Spinner from './Spinner';

function hexToArgbInt(hex: string): number
{
    // hex: #RRGGBB or #AARRGGBB
    let h = hex.replace('#', '');
    let a = 255;
    let r = 0, g = 0, b = 0;
    if (h.length === 8)
    {
        a = parseInt(h.slice(0, 2), 16);
        r = parseInt(h.slice(2, 4), 16);
        g = parseInt(h.slice(4, 6), 16);
        b = parseInt(h.slice(6, 8), 16);
    } else if (h.length === 6)
    {
        r = parseInt(h.slice(0, 2), 16);
        g = parseInt(h.slice(2, 4), 16);
        b = parseInt(h.slice(4, 6), 16);
    }
    return (a << 24) | (r << 16) | (g << 8) | b;
}

function argbIntToHex(argb: number): string
{
    //const a = ((argb >> 24) & 0xFF).toString(16).padStart(2, '0');
    const r = ((argb >> 16) & 0xFF).toString(16).padStart(2, '0');
    const g = ((argb >> 8) & 0xFF).toString(16).padStart(2, '0');
    const b = (argb & 0xFF).toString(16).padStart(2, '0');
    return `#${r}${g}${b}`;
}

export default function ListColorPicker({
    boardId,
    listId,
    currentColor,
    onColorChanged,
    onClose
}: {
    boardId: string;
    listId: string;
    currentColor: number;
    title: string;
    position: number;
    onColorChanged: (color: number) => void;
    onClose: () => void;
})
{
    const [color, setColor] = useState(argbIntToHex(currentColor));
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) =>
    {
        setColor(e.target.value);
    };

    const handleSubmit = async (e: React.FormEvent) =>
    {
        e.preventDefault();
        setError(null);
        setLoading(true);

        const argb = hexToArgbInt(color);
        try
        {
            const patchOps = [
                { op: 'replace', path: '/colorArgb', value: argb }
            ];
            await api.patch(
                `/api/boards/${boardId}/lists/${listId}`,
                patchOps,
                { headers: { 'Content-Type': 'application/json-patch+json' } }
            );

            onColorChanged(argb);
            onClose();
        }
        catch (err: any)
        {
            setError('Failed to update color');
            setLoading(false);
        }
    };

    return (
        <form onSubmit={handleSubmit} className="flex flex-col gap-2 px-4 py-2">
            <label className="flex items-center gap-2">
                <span>Pick color:</span>
                <input
                    type="color"
                    value={color}
                    onChange={handleChange}
                    disabled={loading}
                    className="w-8 h-8 border rounded"
                />
            </label>
            {error && <div className="text-red-600">{error}</div>}
            {loading ? (
                <div className="flex justify-center py-2">
                    <Spinner className="w-6 h-6" />
                </div>
            ) : (
                <div className="flex gap-2 mt-2">
                    <button
                        type="submit"
                        className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600"
                        disabled={loading}
                    >
                        Save
                    </button>
                    <button
                        type="button"
                        className="px-3 py-1 bg-gray-200 rounded hover:bg-gray-300"
                        onClick={onClose}
                        disabled={loading}
                    >
                        Cancel
                    </button>
                </div>
            )}
        </form>
    );
}