import { useEffect, useState, useCallback } from 'react';
import api from '../api';

export interface BoardList
{
    id: string;
    boardId: string;
    title: string;
    position: number;
    colorArgb: number;
}

export function useBoardLists(boardId: string | undefined, keycloak: any, navigate: (path: string) => void, initialized: boolean)
{
    const [lists, setLists] = useState<BoardList[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [fieldErrors, setFieldErrors] = useState<{ [key: string]: string[] }>({});

    const loadLists = useCallback(async () =>
    {
        if (!keycloak?.authenticated || !keycloak.token || !boardId) return;

        setIsLoading(true);
        try
        {
            const listsResponse = await api.get(`/api/boards/${boardId}/lists`);
            const listsData: BoardList[] = listsResponse.data;
            setLists(listsData.sort((a, b) => a.position - b.position));
            setError(null);
        }
        catch
        {
            setError('Failed to load board data. Please try again later.');
        }
        finally
        {
            setIsLoading(false);
        }
    }, [boardId, keycloak?.authenticated, keycloak?.token]);

    useEffect(() =>
    {
        if (!initialized) return;

        if (keycloak?.authenticated && keycloak.token && boardId)
        {
            void loadLists();
        }
        else if (keycloak && !keycloak.authenticated)
        {
            navigate('/');
        }
    }, [initialized, keycloak, boardId, navigate, loadLists]);

    const handleCreateList = async (data: { title: string }) =>
    {
        if (!keycloak.token || !boardId) return false;

        const maxPosition = lists.length > 0 ? Math.max(...lists.map(l => l.position)) : 0;

        try
        {
            var response = await api.post(`/api/boards/${boardId}/lists`, { ...data, position: maxPosition + 1 });

            // Refetch to get authoritative positions/colors
            const listsResponse = await api.get(`/api/boards/${boardId}/lists/${response.data.id}`);
            const newList: BoardList = listsResponse.data;
            setLists(prev => [...prev, newList].sort((a, b) => a.position - b.position));
            setError(null);
            return true;
        }
        catch (error: any)
        {
            if (error.response?.data?.errors)
            {
                const apiErrors = error.response.data.errors;
                setFieldErrors(apiErrors);
            }
            else if (error.response?.data?.message)
            {
                setError(error.response.data.message);
            }
            else
            {
                setError('Failed to create list. Please try again.');
            }
            return false;
        }
    };

    return {
        lists,
        setLists,
        error,
        setError,
        isLoading,
        fieldErrors,
        setFieldErrors,
        handleCreateList,
    };
}