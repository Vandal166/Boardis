import toast from "react-hot-toast";
import api from "../../api";
import { useState } from "react";
import Spinner from "../Spinner";

interface Props
{
    boardId?: string;
    initialMaxPosition?: number;
    onClose: () => void;
}

export default function GenerateListStructureModal({ boardId, initialMaxPosition = 0, onClose }: Props)
{
    const [description, setDescription] = useState('');
    const [loading, setLoading] = useState(false);

    const POSITION_STEP = 1024;

    // Create a list and return its id
    async function createListAndGetId(title: string, position: number): Promise<string | false>
    {
        if (!boardId)
            return false;

        // Try creating list; if API returns id, use it
        try
        {
            const createRes = await api.post(`/api/boards/${boardId}/lists`, { title, position });
            const newId = createRes?.data?.id ?? createRes?.data?.Id;
            if (newId)
                return newId;
        }
        catch (err: any)
        {
            const msg = err && (err.detail || err.title || err.message);
            toast.error(msg || 'Failed to create list.');
        }

        // Fallback: fetch lists and pick the newest match by highest position
        try
        {
            const res = await api.get(`/api/boards/${boardId}/lists`);
            const all = Array.isArray(res.data) ? res.data : [];
            const matches = all.filter((l: any) => l.title === title || l.Title === title);
            const byPosDesc = [...matches].sort((a: any, b: any) => (b.position ?? 0) - (a.position ?? 0));
            return byPosDesc[0]?.id ?? byPosDesc[0]?.Id ?? false;
        }
        catch
        {
            return false;
        }
    }

    // Add a card to a list
    async function addCardToList(listId: string, card: { title: string; description?: string }): Promise<boolean>
    {
        if (!boardId) return false;

        try
        {
            const res = await api.get(`/api/boards/${boardId}/lists/${listId}/cards`);
            const existing = Array.isArray(res.data) ? res.data : [];
            const maxPos = existing.length ? Math.max(...existing.map((c: any) => c.position ?? 0)) : 0;

            await api.post(
                `/api/boards/${boardId}/lists/${listId}/cards`,
                {
                    title: card.title,
                    description: card.description,
                    position: maxPos + 1024.0,
                }
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    const handleGenerate = async () =>
    {
        if (!boardId || !description.trim())
            return;
        setLoading(true);
        try
        {
            let currentMaxPos = initialMaxPosition;

            const res = await api.post('/api/ollama/generate', { description });
            let lists: any[] = [];
            try
            {
                const data = typeof res.data === 'string' ? JSON.parse(res.data) : res.data;
                // Support both PascalCase and camelCase
                lists = Array.isArray(data.lists) ? data.lists : Array.isArray(data.Lists) ? data.Lists : [];
            } catch (err)
            {
                toast.error('Could not parse response structure.');
                setLoading(false);
                return;
            }

            if (lists.length === 0)
            {
                toast.error('No lists found in response.');
                setLoading(false);
                return;
            }

            let successCount = 0;
            let cardCount = 0;
            for (const list of lists)
            {
                // Support both name/Name and cards/Cards
                const listName = list.name ?? list.Name;
                const cardsArr = Array.isArray(list.cards) ? list.cards : Array.isArray(list.Cards) ? list.Cards : [];

                if (listName)
                {
                    currentMaxPos += POSITION_STEP;
                    // Create list and get its id
                    const listId = await createListAndGetId(listName, currentMaxPos);
                    if (listId && typeof listId === 'string')
                    {
                        successCount++;
                        // Add cards if present
                        for (const card of cardsArr)
                        {
                            // Support both title/Title and description/Description
                            const cardTitle = card.title ?? card.Title;
                            const cardDescription = card.description ?? card.Description;
                            if (cardTitle)
                            {
                                const ok = await addCardToList(listId, { title: cardTitle, description: cardDescription });
                                if (ok) cardCount++;
                            }
                        }
                    }
                }
            }
            toast.success(`Created ${successCount} lists and ${cardCount} cards.`, { duration: 6000 });
            console.log('Lists created:', successCount);
            console.log('Cards created:', cardCount);
            onClose();
        }
        catch (err: any)
        {
            const msg = (err && (err.detail || err.title || err.message))
            console.log('Error during generation:', msg);
            toast.error(msg || 'Failed to generate response.');
        }
        finally
        {
            setLoading(false);
        }
    };

    return (
        <div className="fixed inset-0 flex items-center justify-center z-50">
            <div className="bg-white rounded shadow-lg p-6 min-w-[320px]">
                <h2 className="text-lg font-bold mb-4">Generate List Structure</h2>
                <input
                    type="text"
                    value={description}
                    onChange={e => setDescription(e.target.value)}
                    placeholder="Enter your prompt..."
                    className="border px-3 py-2 rounded w-full mb-4"
                    disabled={loading}
                />
                <div className="flex gap-2">
                    <button
                        className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 flex items-center gap-2"
                        onClick={handleGenerate}
                        disabled={loading || !description.trim()}
                    >
                        {loading ? <Spinner className="w-5 h-5" /> : 'Submit'}
                    </button>
                    <button
                        className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                        onClick={onClose}
                        disabled={loading}
                    >
                        Close
                    </button>
                </div>
            </div>
        </div>
    );
}
