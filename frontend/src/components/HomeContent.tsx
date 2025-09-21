import { useKeycloak } from '@react-keycloak/web';
import Carousel from './Carousel';
import { useTranslation } from 'react-i18next';

function HomeContent()
{
  const { keycloak, initialized } = useKeycloak();
  const { t } = useTranslation();

  return (
    <main className="container mx-auto px-6 py-12 min-h-[60vh] flex items-center justify-center">
      <div className="max-w-5xl w-full flex flex-col md:flex-row items-center justify-center">
        <div className="md:w-1/2 mb-8 md:mb-0 flex flex-col justify-center items-center">
          <h2 className="text-5xl font-bold mb-4" style={{ color: 'var(--main-h2-color)' }}>
            {t('mainTitle')}
          </h2>
          <p className="text-lg text-white">
            {t('mainDescription')}
          </p>
          {initialized && !keycloak.authenticated && (
            <form
              className="mt-6 w-full flex"
              onSubmit={e =>
              {
                e.preventDefault();
                keycloak.register({ redirectUri: window.location.origin + '/' });
              }}
            >
              <input
                type="email"
                placeholder={t('emailPlaceholder')}
                className="px-4 py-2 rounded-md border border-gray-800 flex-1 focus:outline-none mr-12 bg-gray-800 text-white"
              />
              <button
                type="submit"
                className="bg-orange-500 text-white px-6 py-2 mr-6 rounded-r-md hover:bg-orange-600 transition"
              >
                {t('getStarted')}
              </button>
            </form>
          )}
        </div>
        <Carousel />
      </div>
    </main>
  );
}

export default HomeContent;