import React, { useEffect, useRef, useState } from 'react';

interface BoardSettingsPanelProps
{
    onClose: () => void;
}

const BoardSettingsPanel: React.FC<BoardSettingsPanelProps> = ({ onClose }) =>
{
    const [show, setShow] = useState(false);
    const panelRef = useRef<HTMLDivElement>(null);

    useEffect(() =>
    {
        // Trigger the animation after mount
        setShow(true);
    }, []);

    // Close on click outside
    useEffect(() =>
    {
        function handleClick(event: MouseEvent)
        {
            if (panelRef.current && !panelRef.current.contains(event.target as Node))
            {
                onClose();
            }
        }
        document.addEventListener('mousedown', handleClick);
        return () => document.removeEventListener('mousedown', handleClick);
    }, [onClose]);

    return (
        <div className="fixed inset-0 z-50">
            {/* Overlay */}
            <div className="absolute inset-0 bg-opacity-0" />
            {/* Modal */}
            <div
                ref={panelRef}
                className={`
                    absolute top-40 right-2 w-80
                    bg-white rounded-xl shadow-2xl border border-gray-200 p-6
                    transition-all duration-300 ease-out
                    ${show ? 'opacity-100 translate-y-0 scale-100' : 'opacity-0 -translate-y-4 scale-95'}
                `}
                style={{ willChange: 'opacity, transform' }}
            >
                {/* Arrow pointing to the gearbox */}
                <div className="absolute -top-2 right-8 w-4 h-4 z-10">
                    <div className="w-4 h-4 bg-white rotate-45 shadow-lg border-t border-l border-gray-200"></div>
                </div>
                <div className="flex justify-between items-center mb-4">
                    <h2 className="text-xl font-bold">Board Settings</h2>
                    <button
                        className="text-gray-400 hover:text-gray-700 text-2xl font-bold"
                        onClick={onClose}
                        aria-label="Close settings"
                    >
                        ×
                    </button>
                </div>
                <div className="space-y-4">
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">About this board</h3>
                        <p className="text-gray-500 text-sm">Board description and details go here.</p>
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">Visibility</h3>
                        <select className="w-full border rounded px-2 py-1">
                            <option>Private</option>
                            <option>Workspace visible</option>
                            <option>Public</option>
                        </select>
                    </div>
                    <div>
                        <h3 className="font-semibold text-gray-700 mb-1">Change wallpaper image</h3>
                        <button className="px-3 py-1 bg-gray-100 rounded hover:bg-gray-200 transition text-sm">
                            Change wallpaper
                        </button>
                    </div>
                    <div>
                        <button className="w-full px-3 py-2 bg-red-100 text-red-700 rounded hover:bg-red-200 transition font-semibold">
                            Delete board
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default BoardSettingsPanel;