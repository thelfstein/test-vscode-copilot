// Script para facilitar a adição de Bearer token no Swagger UI

(function() {
    'use strict';

    const BEARER_TOKEN_KEY = 'swagger-bearer-token';

    // Esperar o Swagger UI carregar completamente
    window.addEventListener('load', function() {
        setTimeout(setupBearerTokenUI, 500);
    });

    function setupBearerTokenUI() {
        // Procurar o topbar do Swagger
        const topbar = document.querySelector('.topbar');
        if (!topbar) return;

        // Criar container para autenticação
        const authContainer = document.createElement('div');
        authContainer.style.cssText = `
            background: #f8f9fa;
            border-bottom: 2px solid #00a86b;
            padding: 15px 20px;
            font-size: 14px;
        `;

        authContainer.innerHTML = `
            <div style="max-width: 1460px; margin: 0 auto;">
                <div style="margin-bottom: 10px; font-weight: bold; color: #1e3a5f;">
                    🔐 Autenticação Bearer JWT
                </div>
                <div style="margin-bottom: 10px; font-size: 12px; line-height: 1.6;">
                    <strong>Credenciais de teste:</strong> Email: <code>admin@barberbooking.com</code> | Senha: <code>Admin@123456</code>
                </div>
                <div style="display: flex; gap: 10px; flex-wrap: wrap; align-items: center;">
                    <input 
                        type="password" 
                        id="bearerTokenInput" 
                        placeholder="Cole o token JWT aqui (da resposta do login)"
                        style="
                            flex: 1;
                            min-width: 250px;
                            padding: 8px 12px;
                            border: 1px solid #ddd;
                            border-radius: 4px;
                            font-family: 'Courier New', monospace;
                            font-size: 11px;
                        ">
                    <button 
                        id="applyBearerBtn" 
                        onclick="window.applyBearerToken()"
                        style="
                            padding: 8px 16px;
                            background-color: #00a86b;
                            color: white;
                            border: none;
                            border-radius: 4px;
                            cursor: pointer;
                            font-weight: bold;
                            white-space: nowrap;
                        ">
                        ✓ Aplicar Token
                    </button>
                    <button 
                        id="clearBearerBtn" 
                        onclick="window.clearBearerToken()"
                        style="
                            padding: 8px 16px;
                            background-color: #dc3545;
                            color: white;
                            border: none;
                            border-radius: 4px;
                            cursor: pointer;
                            font-weight: bold;
                            white-space: nowrap;
                        ">
                        ✗ Limpar
                    </button>
                </div>
                <div id="tokenStatusDiv" style="margin-top: 8px; font-size: 12px; font-weight: bold;"></div>
            </div>
        `;

        topbar.insertBefore(authContainer, topbar.firstChild);

        // Restaurar token do localStorage se existir
        restoreBearerToken();

        // Adicionar interceptador para requisições
        setupRequestInterceptor();
    }

    // Função global para aplicar token
    window.applyBearerToken = function() {
        const tokenInput = document.getElementById('bearerTokenInput');
        const token = tokenInput.value.trim();
        const statusDiv = document.getElementById('tokenStatusDiv');

        if (!token) {
            statusDiv.innerHTML = '<span style="color: #dc3545;">❌ Token vazio!</span>';
            return;
        }

        // Remover "Bearer " se o usuário tiver copiado com o prefixo
        const cleanToken = token.replace(/^Bearer\s+/i, '');

        // Salvar no localStorage
        localStorage.setItem(BEARER_TOKEN_KEY, cleanToken);

        // Atualizar headers de todas as requisições
        injectBearerToAllRequests(cleanToken);

        statusDiv.innerHTML = '<span style="color: #00a86b;">✅ Token aplicado! Será enviado em todas as requisições.</span>';
        
        // Limpar input
        setTimeout(() => {
            tokenInput.value = '';
            statusDiv.innerHTML = '';
        }, 2000);
    };

    // Função global para limpar token
    window.clearBearerToken = function() {
        localStorage.removeItem(BEARER_TOKEN_KEY);
        document.getElementById('bearerTokenInput').value = '';
        document.getElementById('tokenStatusDiv').innerHTML = '<span style="color: #dc3545;">❌ Token removido</span>';
        
        setTimeout(() => {
            document.getElementById('tokenStatusDiv').innerHTML = '';
        }, 2000);
    };

    // Restaurar token do localStorage
    function restoreBearerToken() {
        const savedToken = localStorage.getItem(BEARER_TOKEN_KEY);
        if (savedToken) {
            document.getElementById('bearerTokenInput').value = savedToken;
            injectBearerToAllRequests(savedToken);
            document.getElementById('tokenStatusDiv').innerHTML = 
                '<span style="color: #00a86b;">✅ Token restaurado do storage</span>';
        }
    }

    // Setup para interceptar requisições
    function setupRequestInterceptor() {
        // Guardar a função original de fetch
        const originalFetch = window.fetch;

        // Substituir fetch com versão que adiciona Bearer token
        window.fetch = function(...args) {
            const token = localStorage.getItem(BEARER_TOKEN_KEY);
            
            if (token && args.length > 0) {
                const url = args[0];
                const options = args[1] || {};

                // Não adicionar header para requisições de assets
                if (typeof url === 'string' && !url.includes('.js') && !url.includes('.css') && !url.includes('.png')) {
                    if (!options.headers) {
                        options.headers = {};
                    }
                    options.headers['Authorization'] = 'Bearer ' + token;
                    args[1] = options;
                }
            }

            return originalFetch.apply(this, args);
        };
    }

    // Função para injetar Bearer em requisições via XHR (fallback)
    function injectBearerToAllRequests(token) {
        const originalOpen = XMLHttpRequest.prototype.open;
        const originalSetRequestHeader = XMLHttpRequest.prototype.setRequestHeader;

        XMLHttpRequest.prototype.open = function(method, url, ...rest) {
            this._url = url;
            return originalOpen.apply(this, [method, url, ...rest]);
        };

        XMLHttpRequest.prototype.setRequestHeader = function(header, value) {
            if (header.toLowerCase() === 'authorization') {
                return originalSetRequestHeader.apply(this, [header, 'Bearer ' + token]);
            }
            return originalSetRequestHeader.apply(this, [header, value]);
        };
    }

})();
