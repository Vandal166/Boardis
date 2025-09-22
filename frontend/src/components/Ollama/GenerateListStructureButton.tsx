import { useState } from 'react';
import { SparklesIcon } from '@heroicons/react/24/solid';
import GenerateListStructureModal from './GenerateListStructureModal';

interface Props
{
    boardId?: string;
    initialMaxPosition?: number;
}

export default function GenerateListStructureButton({ boardId, initialMaxPosition }: Props)
{
    const [showModal, setShowModal] = useState(false);

    return (
        <>
            <button
                onClick={() => setShowModal(true)}
                className="transition flex items-center gap-2"
            >
                <SparklesIcon className="w-5 h-5" />
            </button>
            {showModal && (
                <GenerateListStructureModal
                    boardId={boardId}
                    initialMaxPosition={initialMaxPosition ?? 0}
                    onClose={() => setShowModal(false)}
                />
            )}
        </>
    );
}
