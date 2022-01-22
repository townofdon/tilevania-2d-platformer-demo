FROM nginx:stable

WORKDIR /webgl
COPY webgl/ .
VOLUME /webgl

WORKDIR /etc/nginx/conf.d
RUN rm -f default.conf
COPY webgl.conf webgl.conf
