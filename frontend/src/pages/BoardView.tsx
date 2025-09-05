import { useKeycloak } from '@react-keycloak/web';
import axios from 'axios';
import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { EllipsisHorizontalIcon } from '@heroicons/react/24/solid';
import Spinner from '../components/Spinner';

interface BoardList
{
  id: string;
  boardId: string;
  title: string;
  position: number;
  colorArgb: number;
  cards: BoardCard[];
}

interface BoardCard
{
  id: string;
  boardId: string;
  boardListId: string;
  title: string;
  description?: string;
  completedAt?: string;
  position: number;
}

const argbToRgba = (color: number) =>
{
  const a = (color >> 24) & 0xFF;
  const r = (color >> 16) & 0xFF;
  const g = (color >> 8) & 0xFF;
  const b = color & 0xFF;
  return `rgba(${r},${g},${b},${a / 255})`;
};

function BoardView()
{
  const { boardId } = useParams<{ boardId: string }>();
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();
  const [lists, setLists] = useState<BoardList[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() =>
  {
    if (initialized && keycloak.authenticated && keycloak.token && boardId)
    {
      const fetchData = async () =>
      {
        setIsLoading(true);
        try
        {
          // Fetch lists first
          const listsResponse = await axios.get(`/api/boards/${boardId}/lists`, {
            headers: { Authorization: `Bearer ${keycloak.token}` },
          });

          // For each list, fetch its cards using the correct route
          const listsData: Omit<BoardList, 'cards'>[] = listsResponse.data;
          const cardsByList: { [key: string]: BoardCard[] } = {};

          await Promise.all(
            listsData.map(async (list) =>
            {
              try
              {
                const cardsResponse = await axios.get(
                  `/api/boards/${boardId}/lists/${list.id}/cards`,
                  {
                    headers: { Authorization: `Bearer ${keycloak.token}` },
                  }
                );
                cardsByList[list.id] = cardsResponse.data.sort((a: BoardCard, b: BoardCard) => a.position - b.position);
              } catch
              {
                cardsByList[list.id] = [];
              }
            })
          );

          const enrichedLists: BoardList[] = listsData
            .map((list) => ({
              ...list,
              cards: cardsByList[list.id] || [],
            }))
            .sort((a, b) => a.position - b.position);

          setLists(enrichedLists);
          setError(null);
        }
        catch (error)
        {
          console.error('Failed to fetch board data:', error);
          setError('Failed to load board data. Please try again later.');
        }
        finally
        {
          setIsLoading(false);
        }
      };
      fetchData();
    } else if (initialized && !keycloak.authenticated)
    {
      navigate('/'); // Redirect to home if not authenticated
    }
  }, [initialized, keycloak, boardId, navigate]);

  if (!initialized || isLoading)
  {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <Spinner />
      </div>
    );
  }

  if (error)
  {
    return (
      <div className="min-h-screen flex items-center justify-center text-red-600">{error}</div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-100 text-gray-900 font-sans">
      <div className="overflow-x-auto w-full p-6">
        <div
          className="grid gap-4 grid-flow-col auto-cols-[320px]"
        >
          {lists
            .slice()
            .sort((a, b) => a.position - b.position)
            .map((list) => (
              <div
                key={list.id}
                className="bg-gray-200 rounded-lg p-4 flex flex-col w-[320px] min-w-[320px] max-w-[320px]"
              >
                <div
                  className="flex justify-between items-center mb-4 p-2 rounded-t-lg"
                  style={{ backgroundColor: argbToRgba(list.colorArgb) }}
                >
                  <h3 className="text-lg font-semibold text-white">{list.title}</h3>
                  <button aria-label="List settings">
                    <EllipsisHorizontalIcon className="w-5 h-5 text-white" />
                  </button>
                </div>
                <ul className="space-y-3 flex-1">
                  {list.cards.map((card) => (
                    <li
                      key={card.id}
                      className="bg-white p-3 rounded shadow"
                    >
                      <h4 className="font-medium text-blue-800">{card.title}</h4>
                      {card.description && (
                        <p className="text-sm text-gray-600 mt-1">{card.description}</p>
                      )}
                      {card.completedAt && (
                        <p className="text-xs text-green-600 mt-1">
                          Completed: {new Date(card.completedAt).toLocaleString()}
                        </p>
                      )}
                    </li>
                  ))}
                </ul>
                <button className="mt-4 text-blue-600 hover:underline text-left">
                  + Add a card
                </button>
              </div>
            ))}
        </div>
      </div>
    </div>
  );
}

export default BoardView;