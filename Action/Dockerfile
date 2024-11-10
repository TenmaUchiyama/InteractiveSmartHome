# ベースイメージとして Node.js 22-alpine を使用
FROM node:22-alpine

# 必要なパッケージをインストール
RUN apk update && apk add --no-cache bash bash-doc bash-completion

# カラフルなプロンプト設定
ENV TERM xterm-256color
RUN echo "PS1='\e[92m\u\e[0m@\e[94m\h\e[0m:\e[35m\w\e[0m# '" >> ~/.bashrc

# 作業ディレクトリを設定
WORKDIR /action_workspace

# package.json を先にコピーして、依存関係をインストール
COPY ./workspace/package*.json ./
RUN npm install

# ソースコードをコピー（node_modulesは上書きされない）
COPY ./workspace .

# 環境変数を設定
ENV NODE_ENV=docker

# ts-node-devでアプリを起動
CMD ["npx", "ts-node-dev", "--respawn", "-r", "tsconfig-paths/register", "--poll", "src/index.ts"]
