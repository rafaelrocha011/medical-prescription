$raiz = "C:\Projetos\prescricao-medica"

docker run --rm -it `
    --name medical-prescription-angular-dev `
    -p 127.0.0.1:4200:4200 `
    -v "${raiz}:/workspace" `
    -w /workspace/frontend `
    node:22 `
    npm start -- --host 0.0.0.0 --poll=2000 --proxy-config src/proxy.conf.json