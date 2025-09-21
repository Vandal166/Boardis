import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';

function Footer()
{
    const navigate = useNavigate();
    const { t } = useTranslation();

    return (
        <footer className="w-full bg-gray-800 text-white py-12 shadow-2xl z-0">
            <div className="container mx-auto px-6">
                <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
                    <div>
                        <h3 className="text-xl font-semibold mb-4">{t('footerAboutTitle')}</h3>
                        <p className="text-sm">
                            {t('footerAboutDesc')}
                        </p>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">{t('footerQuickLinksTitle')}</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <button
                                    onClick={() => navigate('/')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerHome')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/features')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerFeatures')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/pricing')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerPricing')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/blog')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerBlog')}
                                </button>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">{t('footerSupportTitle')}</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <button
                                    onClick={() => navigate('/help')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerHelpCenter')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/contact')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerContactUs')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/privacy')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerPrivacyPolicy')}
                                </button>
                            </li>
                            <li>
                                <button
                                    onClick={() => navigate('/terms')}
                                    className="hover:underline bg-transparent text-white text-left w-full"
                                >
                                    {t('footerTermsOfService')}
                                </button>
                            </li>
                        </ul>
                    </div>
                    <div>
                        <h3 className="text-xl font-semibold mb-4">{t('footerConnectTitle')}</h3>
                        <ul className="space-y-2 text-sm">
                            <li>
                                <a
                                    href="https://twitter.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    {t('footerTwitter')}
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://facebook.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    {t('footerFacebook')}
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://linkedin.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    {t('footerLinkedIn')}
                                </a>
                            </li>
                            <li>
                                <a
                                    href="https://instagram.com/"
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    className="hover:underline"
                                >
                                    {t('footerInstagram')}
                                </a>
                            </li>
                        </ul>
                    </div>
                </div>
                <div className="mt-8 text-center text-sm">
                    &copy; {new Date().getFullYear()} Boardis. {t('footerCopyright')}
                </div>
            </div>
        </footer>
    );
}

export default Footer;