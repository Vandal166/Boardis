import { createContext, useContext, useEffect, type ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import toast from 'react-hot-toast';
import NotificationToast from '../components/NotificationToast';
import { useKeycloak } from '@react-keycloak/web';

interface SignalRContextType
{
    notificationConnection: signalR.HubConnection;
}

const SignalRContext = createContext<SignalRContextType | undefined>(undefined);

type TokenObj = string | { access_token?: string };

export function SignalRProvider({ children }: { children: ReactNode })
{
    const { keycloak, initialized } = useKeycloak();

    // Move getAccessToken inside the provider to always get the latest token
    function getAccessToken(): string
    {
        const tokenObj: TokenObj = keycloak?.token ?? "";
        const token =
            typeof tokenObj === "string"
                ? tokenObj
                : (tokenObj as { access_token?: string }).access_token || "";
        return token;
    }

    const notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl("http://localhost:5185/generalNotificationHub",
            {
                accessTokenFactory: () => getAccessToken() // always fetch latest token
            })
        .withAutomaticReconnect()
        .build();

    useEffect(() =>
    {
        if (!initialized || !keycloak.authenticated || !keycloak.token)
            return;

        const handleNotification = (notification: any) =>
        {
            console.log('[SignalR] Received notification:', notification);
            if (notification?.type === "Invited")
            {
                toast.custom((t) => (
                    <NotificationToast
                        title={notification.title}
                        byUser={notification.byUser}
                        onView={() =>
                        {
                            toast.dismiss(t.id);
                            window.location.href = `/dashboard/board/${notification.boardId}`;
                        }}
                        onClose={() => toast.dismiss(t.id)}
                        visible={t.visible}
                    />
                ), { duration: 8000, position: "top-center" });
                window.dispatchEvent(new CustomEvent("boardis:invited"));
            }
            else if (notification?.type === "Removed")
            {
                window.dispatchEvent(new CustomEvent("boardis:removed", { detail: notification }));
            }
        };

        const startConnection = async () =>
        {
            try
            {
                if (notificationConnection.state === signalR.HubConnectionState.Disconnected)
                {
                    console.log('[SignalR] Attempting to start notification connection...');
                    await notificationConnection.start();
                    console.log('[SignalR] Notification connection started');
                }
                else
                {
                    console.log('[SignalR] Notification connection already started');
                }
            }
            catch (err)
            {
                console.error('[SignalR] Error starting notification connection:', err);
            }
            // Register event handler after connection is started
            notificationConnection.on("ReceiveNotification", handleNotification);
            console.log('[SignalR] Notification handler registered');
        };

        // Listen for connection state changes
        notificationConnection.onclose((error) =>
        {
            console.log('[SignalR] Notification connection closed', error);
        });
        notificationConnection.onreconnecting((error) =>
        {
            console.log('[SignalR] Notification connection reconnecting', error);
        });
        notificationConnection.onreconnected((connectionId) =>
        {
            console.log('[SignalR] Notification connection reconnected', connectionId);
        });

        startConnection();

        return () =>
        {
            notificationConnection.off("ReceiveNotification", handleNotification);
            console.log('[SignalR] Notification handler unregistered');
            // keep alive for global
        };
    }, [notificationConnection, initialized, keycloak.authenticated, keycloak.token]);

    return (
        <SignalRContext.Provider value={{ notificationConnection }}>
            {children}
        </SignalRContext.Provider>
    );
}

export const useSignalR = () =>
{
    const context = useContext(SignalRContext);
    if (!context) throw new Error('useSignalR must be used within SignalRProvider');
    return context;
};