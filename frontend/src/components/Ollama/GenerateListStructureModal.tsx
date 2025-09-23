import toast from "react-hot-toast";
import api from "../../api";
import { useState, useEffect, useRef } from "react";
import Spinner from "../Spinner";
import { useTranslation } from "react-i18next";

interface Props
{
    boardId?: string;
    initialMaxPosition?: number;
    onClose: () => void;
    onAwaitingChange?: (awaiting: boolean) => void; // NEW: notify parent about awaiting state
}

export default function GenerateListStructureModal({ boardId, initialMaxPosition = 0, onClose, onAwaitingChange }: Props)
{
    const [description, setDescription] = useState('');
    const [loading, setLoading] = useState(false);
    // Modal animation and click-outside handling
    const [show, setShow] = useState(false);
    const [visible, setVisible] = useState(true);
    const panelRef = useRef<HTMLDivElement>(null);

    const { t } = useTranslation();
    useEffect(() => setShow(true), []);
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
                setVisible(false);
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, []);
    useEffect(() =>
    {
        if (!visible)
        {
            const timeout = setTimeout(() => onClose(), 50);
            return () => clearTimeout(timeout);
        }
    }, [visible, onClose]);

    const POSITION_STEP = 1024;
    const MAX_CHARS = 2000;

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
            const msg = err && (err.data?.errors || err.detail || err.title || err.message);
            toast.error(msg || t('generateListModalCreateListFailed'));
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
        if (!boardId || !description.trim() || loading)
            return;
        setLoading(true);

        // Notify parent and close immediately
        onAwaitingChange?.(true);
        setVisible(false);

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
                toast.error(t('generateListModalParseFailed'));
                return;
            }

            if (lists.length === 0)
            {
                toast.error(t('generateListModalNoListsFound'));
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
            toast.success(
                t('generateListModalSuccess', { lists: successCount, cards: cardCount }),
                { duration: 6000 }
            );
            console.log('Lists created:', successCount);
            console.log('Cards created:', cardCount);
        }
        catch (err: any)
        {
            // Show validation errors if present
            if (err?.response?.data?.errors)
            {
                if (err.response.data.errors.Description && Array.isArray(err.response.data.errors.Description))
                {
                    toast.error(err.response.data.errors.Description[0]);
                }
                else
                {
                    toast.error(err.response.data.title || t('generateListModalValidationError'));
                }
            }
            else
            {
                const msg = (err && (err.detail || err.title || err.message));
                console.log('Error during generation:', msg);
                toast.error(msg || t('generateListModalFailed'));
            }
        }
        finally
        {
            onAwaitingChange?.(false); // notify parent request ended
            setLoading(false);
        }
    };

    const handleRequestClose = () => setVisible(false);

    return (
        <div className="fixed inset-0 z-50 cursor-auto">
            <div className="absolute inset-0 bg-black opacity-40" />
            <div
                ref={panelRef}
                className={`
                    absolute top-40 left-1/2 -translate-x-1/2 w-full max-w-lg
                    bg-white rounded-xl shadow-2xl border border-gray-200 p-6
                    transition-all duration-300 ease-out
                    ${show && visible ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={{ willChange: 'opacity, transform' }}
            >
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">{t('generateListModalTitle')}</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={handleRequestClose}
                        aria-label={t('generateListModalCloseAria')}
                    >
                        ×
                    </button>
                </div>

                <div className="mb-4">
                    {/* Notes section */}
                    <div className="mb-2 text-xs text-gray-600 space-y-1">
                        <div>• {t('generateListModalNoteEnglishOnly')}</div>
                        <div>• {t('generateListModalNoteMoreDetails')}</div>
                        <div>• {t('generateListModalNoteBeSpecific')}</div>
                        <div>• {t('generateListModalNoteAvoidAmbiguity')}</div>
                    </div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">{t('generateListModalPromptLabel')}</label>
                    <textarea
                        value={description}
                        onChange={e => setDescription(e.target.value.slice(0, MAX_CHARS))}
                        placeholder={t('generateListModalPromptPlaceholder')}
                        className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 resize-none"
                        disabled={loading}
                        rows={5}
                        style={{ overflow: 'auto' }}
                        maxLength={MAX_CHARS}
                    />
                    <div
                        className={`mt-1 text-xs text-right ${description.length >= MAX_CHARS * 0.9 ? 'text-amber-600' : 'text-gray-500'}`}
                        aria-live="polite"
                    >
                        {description.length} / {MAX_CHARS}
                    </div>
                </div>

                <div className="flex justify-end gap-2">
                    <button
                        className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-gray-700"
                        onClick={handleRequestClose}
                        disabled={loading}
                    >
                        {t('generateListModalCancel')}
                    </button>
                    <button
                        className="px-4 py-2 rounded bg-blue-600 hover:bg-blue-700 text-white font-semibold flex items-center gap-2 disabled:opacity-60 disabled:cursor-not-allowed"
                        onClick={handleGenerate}
                        disabled={loading || !description.trim()}
                    >
                        {loading ? <Spinner className="w-5 h-5" /> : t('generateListModalSubmit')}
                    </button>
                </div>
            </div>
        </div>
    );
}
