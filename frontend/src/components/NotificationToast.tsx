import { useTranslation } from 'react-i18next';

interface NotificationToastProps
{
    title: string;
    byUser: string;
    onView?: () => void;
    onClose?: () => void;
    visible?: boolean; // new prop for animation
}

export default function NotificationToast({
    title,
    byUser,
    onView,
    onClose,
    visible = true, // default to true
}: NotificationToastProps)
{
    // Get first two letters of byUser (username)
    const initials = byUser
        ? byUser.trim().substring(0, 2).toUpperCase()
        : "??";
    const { t } = useTranslation();

    return (
        <div
            className={`${visible ? 'animate-custom-enter' : 'animate-custom-leave'
                } max-w-md w-full bg-white shadow-lg rounded-lg pointer-events-auto flex ring-1 ring-black ring-opacity-5`}
        >
            <div className="flex-1 w-0 p-4">
                <div className="flex items-start">
                    <div className="flex-shrink-0 pt-0.5">
                        <div className="h-10 w-10 rounded-full bg-indigo-600 flex items-center justify-center text-white font-bold text-lg select-none">
                            {initials}
                        </div>
                    </div>
                    <div className="ml-3 flex-1">
                        <p className="text-sm font-medium text-gray-900">
                            {title}
                        </p>
                        <div className="mt-1 text-sm text-gray-500">
                            {t('notificationInvitedBy', { user: byUser })}
                        </div>
                    </div>
                </div>
            </div>
            <div className="flex flex-col border-l border-gray-200">
                {onView && (
                    <button
                        onClick={onView}
                        className="w-full border border-transparent rounded-none p-4 flex items-center justify-center text-sm font-medium text-indigo-600 hover:text-indigo-500 focus:outline-none focus:ring-2 focus:ring-indigo-500"
                    >
                        {t('notificationViewBoard')}
                    </button>
                )}
                <button
                    onClick={onClose}
                    className="w-full border border-transparent rounded-none rounded-r-lg p-4 flex items-center justify-center text-sm font-medium text-gray-600 hover:text-gray-500 focus:outline-none focus:ring-2 focus:ring-gray-500"
                >
                    {t('notificationClose')}
                </button>
            </div>
        </div>
    );
}
