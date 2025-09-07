import './App.css';
import { useKeycloak } from '@react-keycloak/web';
import { Toaster } from 'react-hot-toast';
import { Outlet } from 'react-router-dom';
import { setTokenGetter } from './api';
import { useEffect } from 'react';

function App()
{
  const { keycloak, initialized } = useKeycloak();

  // Set the token getter once Keycloak is available
  useEffect(() =>
  {
    setTokenGetter(() => keycloak?.token);
  }, [keycloak]);

  return (
    <div>
      <Toaster position="top-right" />
      {initialized ? ( // waiting for keycloak to initialize, all routes do not render until then
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