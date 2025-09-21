import React, { useState, useEffect, useRef } from 'react';
import Spinner from './Spinner';
import api from '../api';
import toast from 'react-hot-toast';
import { useTranslation } from 'react-i18next';

type BoardCard = {
    id: string;
    boardId: string;
    boardListId: string;
    title: string;
    description?: string;
    position: number;
    completedAt?: string;
};

interface CardDetailsModalProps
{
    card: BoardCard;
    onClose: () => void;
    onUpdated?: (updated: BoardCard) => void;
}

const CardDetailsModal: React.FC<CardDetailsModalProps> = ({ card, onClose, onUpdated }) =>
{
    const { t } = useTranslation();
    const [title, setTitle] = useState(card.title);
    const [description, setDescription] = useState(card.description ?? '');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [fieldErrors, setFieldErrors] = useState<{ [key: string]: string[] }>({});
    const [show, setShow] = useState(false);
    const [visible, setVisible] = useState(true); // controls animation for exit
    const panelRef = useRef<HTMLDivElement>(null);
    const descRef = useRef<HTMLTextAreaElement>(null);

    // Autoexpand description textarea on modal show
    useEffect(() =>
    {
        if (descRef.current)
        {
            descRef.current.style.height = 'auto';
            const maxHeight = 300; // px
            const newHeight = Math.min(descRef.current.scrollHeight, maxHeight);
            descRef.current.style.height = newHeight + 'px';
        }
    }, []);

    // Expand textarea on input, but limit max height
    const handleDescriptionChange = (e: React.ChangeEvent<HTMLTextAreaElement>) =>
    {
        setDescription(e.target.value);
        if (descRef.current)
        {
            descRef.current.style.height = 'auto';
            const maxHeight = 300; // px
            const newHeight = Math.min(descRef.current.scrollHeight, maxHeight);
            descRef.current.style.height = newHeight + 'px';
        }
    };

    const handleSave = async () =>
    {
        setLoading(true);
        setError(null);
        setSuccess(null);
        setFieldErrors({});
        try
        {
            const patchOps = [];
            if (title !== card.title)
                patchOps.push({ op: 'replace', path: '/title', value: title });
            if (description !== (card.description ?? ''))
                patchOps.push({ op: 'replace', path: '/description', value: description });
            if (patchOps.length === 0)
            {
                setSuccess(t('cardDetailsNoChanges'));
                setLoading(false);
                return;
            }
            await api.patch(
                `/api/boards/${card.boardId}/lists/${card.boardListId}/cards/${card.id}`,
                patchOps,
                { headers: { 'Content-Type': 'application/json-patch+json' } }
            );
            toast.success(t('cardDetailsUpdateSuccess'));

            if (onUpdated)
                onUpdated({ ...card, title, description });
        }
        catch (err: any)
        {
            let message = t('cardDetailsUpdateFailed');
            if (err.response?.data?.errors)
            {
                setFieldErrors(err.response.data.errors);
                message = '';
            }
            else if (err.response?.data?.detail)
                message = err.response.data.detail;
            else if (err.response?.data?.title)
                message = err.response.data.title;
            setError(message);
        }
        finally
        {
            setLoading(false);
        }
    };

    useEffect(() => setShow(true), []);

    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                setVisible(false);
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, []);

    // Handle transition end for exit
    useEffect(() =>
    {
        if (!visible)
        {
            // Wait for transition duration before closing
            const timeout = setTimeout(() =>
            {
                onClose();
            }, 50);
            return () => clearTimeout(timeout);
        }
    }, [visible, onClose]);

    // When close button or cancel is clicked
    const handleRequestClose = () =>
    {
        setVisible(false);
    };

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
                    <h2 className="text-xl font-bold">{t('cardDetailsTitle')}</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={handleRequestClose}
                        aria-label={t('cardDetailsCloseAria')}
                    >
                        ×
                    </button>
                </div>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">{t('cardDetailsLabelTitle')}</label>
                    <input
                        className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400"
                        value={title}
                        onChange={e => setTitle(e.target.value)}
                        disabled={loading}
                    />
                    {fieldErrors.Title && Array.isArray(fieldErrors.Title) && (
                        <div className="text-red-600 text-sm mt-1">{fieldErrors.Title[0]}</div>
                    )}
                </div>
                <div className="mb-4">
                    <label className="block text-sm font-medium text-gray-700 mb-1">{t('cardDetailsLabelDescription')}</label>
                    <textarea
                        ref={descRef}
                        className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-400 resize-none"
                        value={description}
                        onChange={handleDescriptionChange}
                        rows={4}
                        disabled={loading}
                        style={{ overflow: 'auto' }}
                    />
                    {fieldErrors.Description && Array.isArray(fieldErrors.Description) && (
                        <div className="text-red-600 text-sm mt-1">{fieldErrors.Description[0]}</div>
                    )}
                </div>
                {card.completedAt && (
                    <div className="mb-2 text-xs text-green-600">
                        {t('cardDetailsCompleted', { date: new Date(card.completedAt).toLocaleString() })}
                    </div>
                )}
                {error && !Object.keys(fieldErrors).length && (
                    <div className="text-red-600 text-sm mb-2">{error}</div>
                )}
                {success && <div className="text-green-600 text-sm mb-2">{success}</div>}
                <div className="flex justify-end gap-2">
                    <button
                        className="px-4 py-2 rounded bg-gray-200 hover:bg-gray-300 text-gray-700"
                        onClick={handleRequestClose}
                        disabled={loading}
                    >
                        {t('cardDetailsCancel')}
                    </button>
                    <button
                        className="px-4 py-2 rounded bg-blue-600 hover:bg-blue-700 text-white font-semibold"
                        onClick={handleSave}
                        disabled={loading}
                    >
                        {loading ? <Spinner /> : t('cardDetailsSave')}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default CardDetailsModal;
