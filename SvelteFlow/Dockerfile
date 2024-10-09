FROM node:22-alpine


RUN apk update && apk add --no-cache bash bash-doc bash-completion

ENV TERM xterm-256color

RUN echo "PS1='\e[92m\u\e[0m@\e[94m\h\e[0m:\e[35m\w\e[0m# '" >> ~/.bashrc
    
