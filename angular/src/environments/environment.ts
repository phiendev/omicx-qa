import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

const oAuthConfig = {
  issuer: 'https://localhost:44331/',
  redirectUri: baseUrl,
  clientId: 'QA_App',
  responseType: 'code',
  scope: 'offline_access QA',
  requireHttps: true,
};

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'QA',
  },
  oAuthConfig,
  apis: {
    default: {
      url: 'https://localhost:44331',
      rootNamespace: 'Omicx.QA',
    },
    AbpAccountPublic: {
      url: oAuthConfig.issuer,
      rootNamespace: 'AbpAccountPublic',
    },
  },
} as Environment;
