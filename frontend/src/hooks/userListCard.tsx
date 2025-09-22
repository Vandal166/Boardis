import { useState, useEffect } from "react";
import api from "../api";
import { useTranslation } from 'react-i18next';


export interface ListCard
{
    id: string;
    boardId: string;
    boardListId: string;
    title: string;
    description?: string;
    completedAt?: string;
    position: number;
}


export function useUserListCards(
    boardId: string | undefined,
    listId: string | undefined,
    keycloak: any,
    navigate: (path: string) => void,
    initialized: boolean
)
{
    const [cards, setCards] = useState<ListCard[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);
    const [fieldErrors, setFieldErrors] = useState<{ [key: string]: string[] }>({});
    const { t } = useTranslation();

    const fetchData = async () =>
    {
        if (!initialized)
            return; // waiting for keycloak to initialize

        if (keycloak?.authenticated && keycloak.token && listId)
        {
            setIsLoading(true);
            try
            {
                const response = await api.get(`/api/boards/${boardId}/lists/${listId}/cards`);
                setCards(response.data);
                setError(null);
            } catch (error)
            {
                console.error('Failed to fetch list cards:', error);
                setError(t('userListCardFailed'));
            } finally
            {
                setIsLoading(false);
            }
        }
        else if (initialized && !keycloak?.authenticated)
        {
            navigate('/'); // Redirect to home if not authenticated
        }
    };

    useEffect(() =>
    {
        fetchData();
    }, [keycloak, listId, navigate, initialized]);

    async function handleAddCardToList(existingListId: string, data: { title: string; description?: string }): Promise<boolean | undefined>
    {
        if (!keycloak.token || !existingListId)
            return;

        // Find max position among current cards
        const maxPosition = cards.length > 0
            ? Math.max(...cards.map(c => c.position ?? 0))
            : 0;

        try
        {
            await api.post(
                `/api/boards/${boardId}/lists/${existingListId}/cards`,
                {
                    title: data.title,
                    description: data.description,
                    position: maxPosition + 1024.0,
                }
            );
            await fetchData(); // Refetch cards after adding
            return true;
        }
        catch (error: any)
        {
            if (error.response?.data?.errors)
            {
                setFieldErrors(error.response.data.errors);
            }
            else if (error.response?.data?.message)
            {
                setError(error.response.data.message);
            }
            else
            {
                setError(t('userListCardAddFailed'));
            }
            return false;
        }
    };


    const handleAddCard = async (data: { title: string; description?: string }) =>
    {
        if (!keycloak.token || !listId)
            return;

        // Find max position among current cards
        const maxPosition = cards.length > 0
            ? Math.max(...cards.map(c => c.position ?? 0))
            : 0;

        try
        {
            await api.post(
                `/api/boards/${boardId}/lists/${listId}/cards`,
                {
                    title: data.title,
                    description: data.description,
                    position: maxPosition + 1024.0,
                }
            );
            await fetchData(); // Refetch cards after adding
            return true;
        }
        catch (error: any)
        {
            if (error.response?.data?.errors)
            {
                setFieldErrors(error.response.data.errors);
            }
            else if (error.response?.data?.message)
            {
                setError(error.response.data.message);
            }
            else
            {
                setError(t('userListCardAddFailed'));
            }
            return false;
        }
    };

    return { cards, setCards, error, isLoading, fieldErrors, setFieldErrors, handleAddCard, handleAddCardToList, refetch: fetchData };
}