import { ReactKeycloakProvider } from '@react-keycloak/web';
import Keycloak from 'keycloak-js';
import type { ReactNode } from 'react';
import { setTokenGetter } from './api';

const keycloak = new Keycloak({
  url: '/auth', // proxied path
  realm: 'BoardisRealm',
  clientId: 'boardis-web',
});

interface KeycloakProviderProps
{
  children: ReactNode;
}

// Set the token getter for axios
setTokenGetter(() => keycloak.token);

export const KeycloakProvider = ({ children }: KeycloakProviderProps) =>
{
  const handleOnEvent = (event: string, error?: any) =>
  {
    console.log('Keycloak event:', event, error || '');
    if (event === 'onAuthError' || event === 'onAuthFailure')
    {
      console.error('Keycloak auth error:', error);
    }
    if (event === 'onReady')
    {
      console.log('Keycloak initialized, authenticated:', keycloak.authenticated);
    }
  };

  return (
    <ReactKeycloakProvider
      authClient={keycloak}
      initOptions={{
        checkLoginIframe: false
      }}
      onEvent={handleOnEvent}
    >
      {children}
    </ReactKeycloakProvider>
  );
};