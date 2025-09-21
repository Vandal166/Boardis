import './App.css';
import { useKeycloak } from '@react-keycloak/web';
import { Toaster } from 'react-hot-toast';
import { Outlet, useNavigate } from 'react-router-dom';
import { setTokenGetter } from './api';
import { useEffect } from 'react';

function App()
{
  const { keycloak, initialized } = useKeycloak();
  const navigate = useNavigate();

  // setting the token getter once Keycloak is available
  useEffect(() =>
  {
    setTokenGetter(() => keycloak?.token);
  }, [keycloak]);

  useEffect(() =>
  {
    const handler = (e: any) =>
    {
      if (e.detail?.path)
      {
        navigate(e.detail.path);
      }
    };
    window.addEventListener('boardis:navigate', handler);
    return () => window.removeEventListener('boardis:navigate', handler);
  }, [navigate]);

  return (
    <div>
      <Toaster
        position="top-right"
        containerStyle={{ top: 60, right: 0 }} // moves the container lower
      />
      {initialized ? (
        <Outlet />
      ) : (
        <div>
          <Outlet />
        </div>
      )}
    </div>
  );
}

export default App;