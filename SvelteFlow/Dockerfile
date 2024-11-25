FROM node:22-alpine


RUN apk update && apk add --no-cache bash bash-doc bash-completion

    

ENV TERM xterm-256color

RUN echo "PS1='\e[92m\u\e[0m@\e[94m\h\e[0m:\e[35m\w\e[0m# '" >> ~/.bashrc

ENV NODE_ENV development
WORKDIR /svelte_flow_workspace

COPY ./workspace/package.json /svelte_flow_workspace/

RUN npm install


COPY ./workspace /svelte_flow_workspace

CMD ["npm", "run" , "dev"]
