import { useEffect, useState } from 'react';
import axios from 'axios';

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

    useEffect(() =>
    {
        if (!initialized)
            return; // waiting for keycloak to initialize

        if (keycloak?.authenticated && keycloak.token && boardId)
        {
            const fetchData = async () =>
            {
                setIsLoading(true);
                try
                {
                    const listsResponse = await axios.get(`/api/boards/${boardId}/lists`, {
                        headers: { Authorization: `Bearer ${keycloak.token}` },
                    });

                    const listsData: BoardList[] = listsResponse.data;

                    setLists(listsData.sort((a, b) => a.position - b.position));
                    setError(null);
                }
                catch (error)
                {
                    setError('Failed to load board data. Please try again later.');
                }
                finally
                {
                    setIsLoading(false);
                }
            };

            fetchData();
        }
        else if (keycloak && !keycloak.authenticated)
        {
            navigate('/');
        }
    }, [keycloak, boardId, navigate]);

    const handleCreateList = async (data: { title: string }) =>
    {
        if (!keycloak.token || !boardId) return false;

        const maxPosition = lists.length > 0 ? Math.max(...lists.map(l => l.position)) : 0;

        try
        {
            const response = await axios.post(
                `/api/boards/${boardId}/lists`,
                { ...data, position: maxPosition + 1 },
                {
                    headers: { Authorization: `Bearer ${keycloak.token}` },
                }
            );
            const newList = response.data;

            setLists((prev) => [...prev, newList].sort((a, b) => a.position - b.position));
            return true;
        } catch (error: any)
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