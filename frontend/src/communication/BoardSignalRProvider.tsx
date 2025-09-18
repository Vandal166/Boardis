import { createContext, useContext, useEffect, type ReactNode } from 'react';
import * as signalR from '@microsoft/signalr';
import { useKeycloak } from '@react-keycloak/web';

interface BoardSignalRContextType
{
    boardHubConnection: signalR.HubConnection;
}

const BoardSignalRContext = createContext<BoardSignalRContextType | undefined>(undefined);

// Singleton connection (created once outside components)
let boardHubConnection: signalR.HubConnection | null = null;

export function BoardSignalRProvider({ children }: { children: ReactNode })
{
    const { keycloak, initialized } = useKeycloak();

    // Lazy-create the singleton if not exists
    if (!boardHubConnection)
    {
        boardHubConnection = new signalR.HubConnectionBuilder()
            .withUrl("http://localhost:5185/boardHub", {
                accessTokenFactory: () =>
                {
                    const token = keycloak?.token ?? "";
                    return token;
                }
            })
            .withAutomaticReconnect()
            .build();
    }

    useEffect(() =>
    {
        const startConnection = async () =>
        {
            if (!initialized || !keycloak.authenticated)
            {
                console.log('[SignalR][BoardHub] Keycloak not ready—delaying start');
                return;
            }
            try
            {
                if (boardHubConnection?.state === signalR.HubConnectionState.Disconnected)
                {
                    console.log('[SignalR][BoardHub] Attempting to start connection...');
                    await boardHubConnection.start();
                    console.log('[SignalR][BoardHub] Connection started');
                }
            } catch (err)
            {
                console.error('[SignalR][BoardHub] Error starting connection:', err);
            }
        };

        startConnection();

        // Optional: Handle reconnects or closes if needed
        return () =>
        {
            // Don't stop here—keep alive globally; stop only on logout if desired
        };
    }, [initialized, keycloak.authenticated]);  // Depend on Keycloak state

    return (
        <BoardSignalRContext.Provider value={{ boardHubConnection }}>
            {children}
        </BoardSignalRContext.Provider>
    );
}

export const useBoardSignalR = () =>
{
    const context = useContext(BoardSignalRContext);
    if (!context) throw new Error('useBoardSignalR must be used within BoardSignalRProvider');
    return context.boardHubConnection;
};