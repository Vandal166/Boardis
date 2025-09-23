import { useState, useEffect } from 'react';
import { SparklesIcon } from '@heroicons/react/24/solid';
import GenerateListStructureModal from './GenerateListStructureModal';
import { useTranslation } from 'react-i18next';

interface Props
{
    boardId?: string;
    initialMaxPosition?: number;
    showPromo?: boolean; // new
}

export default function GenerateListStructureButton({ boardId, initialMaxPosition, showPromo }: Props)
{
    const [showModal, setShowModal] = useState(false);
    const [promoVisible, setPromoVisible] = useState(!!showPromo);
    // New: control entry animation
    const [promoAnimIn, setPromoAnimIn] = useState(false);
    // NEW: remember awaiting state between modal mounts
    const [awaitingAI, setAwaitingAI] = useState(false);
    const { t } = useTranslation();

    useEffect(() =>
    {
        // Re-show when parent requests it
        setPromoVisible(!!showPromo);
        if (showPromo)
        {
            const t = setTimeout(() => setPromoVisible(false), 5000);
            return () => clearTimeout(t);
        }
    }, [showPromo]);

    // Trigger animation on first render of promo
    useEffect(() =>
    {
        if (promoVisible)
        {
            setPromoAnimIn(false);
            const raf = requestAnimationFrame(() => setPromoAnimIn(true));
            return () => cancelAnimationFrame(raf);
        }
        setPromoAnimIn(false);
    }, [promoVisible]);

    return (
        <div
            className={`relative inline-flex rounded-full border transition-[box-shadow,border-color] duration-300
            ${promoVisible ? 'border-amber-300/60 shadow-[0_0_24px_3px_rgba(251,191,36,0.45)]' : 'border-transparent shadow-none'}`}
        >
            {/* Soft pulse around the button when promo is visible */}
            {promoVisible && (
                <span
                    className="pointer-events-none absolute inset-0 rounded-full ring-2 ring-amber-300/60 blur-[1px] animate-pulse"
                    aria-hidden="true"
                />
            )}
            <button
                onClick={() => { if (!awaitingAI) { setShowModal(true); setPromoVisible(false); } }}
                className="transition-all duration-200 ease-out flex items-center gap-2 rounded-full p-2 hover:scale-110 hover:rotate-3 focus:outline-none focus:ring-2 focus:ring-amber-300/60 disabled:opacity-50 disabled:cursor-not-allowed"
                title={t('generateListButtonTitle')}
                aria-label={t('generateListButtonAria')}
                disabled={awaitingAI}
            >
                <SparklesIcon className="w-5 h-5" style={{ color: '#ffc312' }} />
                {awaitingAI && <span className="text-xs text-gray-700 ml-1">{t('generateListButtonAwaiting')}</span>}
            </button>

            {promoVisible && (
                <div
                    className={[
                        // Positioning
                        'absolute top-10 -right-2 z-50 origin-top-right',
                        // Animated entry
                        'transition-all duration-300 ease-out',
                        promoAnimIn ? 'opacity-100 scale-100 translate-y-0' : 'opacity-0 scale-95 -translate-y-2'
                    ].join(' ')}
                >
                    {/* Glow backdrop */}
                    <div className="absolute -inset-1 rounded-xl bg-gradient-to-br from-amber-300/25 to-fuchsia-400/20 blur-md pointer-events-none animate-pulse" aria-hidden="true" />
                    {/* Card: add overflow-hidden so inner part of arrow is clipped */}
                    <div className="relative rounded-xl border border-white/30 bg-gradient-to-br from-white/95 to-white/80 backdrop-blur shadow-xl shadow-amber-100/40 px-3 py-2 overflow-hidden">
                        <div className="flex items-center gap-2">
                            <SparklesIcon className="w-12 h-12" style={{ color: '#ffc312' }} />
                            <span className="text-sm font-semibold text-gray-800">
                                {t('generateListButtonPromo')}
                            </span>
                            <button
                                className="ml-2 text-gray-500 hover:text-gray-700 transition-colors"
                                onClick={() => setPromoVisible(false)}
                                aria-label={t('generateListButtonPromoCloseAria')}
                                title={t('generateListButtonPromoCloseTitle')}
                            >
                                ×
                            </button>
                        </div>
                    </div>
                </div>
            )}

            {showModal && (
                <GenerateListStructureModal
                    boardId={boardId}
                    initialMaxPosition={initialMaxPosition ?? 0}
                    onClose={() => setShowModal(false)}
                    onAwaitingChange={setAwaitingAI}
                />
            )}
        </div>
    );
}
