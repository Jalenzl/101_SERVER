FROM mcr.microsoft.com/dotnet/aspnet:10.0-resolute-amd64

# 针对 Ubuntu 26.04 (DEB822 格式) 切换至阿里云镜像源
RUN sed -i 's/archive.ubuntu.com/mirrors.aliyun.com/g' /etc/apt/sources.list.d/ubuntu.sources && \
    sed -i 's/security.ubuntu.com/mirrors.aliyun.com/g' /etc/apt/sources.list.d/ubuntu.sources

# 安装系统基础依赖
RUN apt-get update && \
    apt-get install -y \
        libfontconfig1 procps libgdiplus software-properties-common \
        fontconfig ttf-mscorefonts-installer apt-utils libc6-dev \
        libgl1 libglib2.0-0 libsm6 libxrender1 libxext6 libgomp1 && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 安装 Python 3.12 编译依赖，从源码编译安装（启用 shared library，pythonnet 必需）
RUN apt-get update && \
    apt-get install -y \
        build-essential wget \
        zlib1g-dev libncurses5-dev libgdbm-dev libnss3-dev \
        libssl-dev libreadline-dev libffi-dev libsqlite3-dev \
        libbz2-dev liblzma-dev && \
    wget https://www.python.org/ftp/python/3.12.8/Python-3.12.8.tgz && \
    tar -xf Python-3.12.8.tgz && \
    cd Python-3.12.8 && \
    ./configure --enable-optimizations --enable-shared --prefix=/usr/local && \
    make -j "$(nproc)" && \
    make altinstall && \
    cd .. && rm -rf Python-3.12.8* && \
    ldconfig && \
    apt-get purge -y build-essential wget && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# 创建 python3 / pip3 符号链接
RUN ln -sf /usr/local/bin/python3.12 /usr/local/bin/python3 && \
    ln -sf /usr/local/bin/pip3.12 /usr/local/bin/pip3

# ============================================================
# pythonnet 所需的 Python 环境变量
# ============================================================
# PYTHONNET_PYDLL  - pythonnet 加载的 Python 共享库路径（最关键）
# PYTHONHOME       - Python 安装根目录前缀
# PYTHONPATH       - Python 模块搜索路径（标准库 + site-packages）
# LD_LIBRARY_PATH  - 动态链接器搜索 libpython3.12.so 的路径
# PATH             - 确保 python3.12 可执行文件可被找到
# ============================================================
ENV PYTHONNET_PYDLL=/usr/local/lib/libpython3.12.so
ENV PYTHONHOME=/usr/local
ENV PYTHONPATH=/usr/local/lib/python3.12:/usr/local/lib/python3.12/lib-dynload:/usr/local/lib/python3.12/site-packages
ENV LD_LIBRARY_PATH=/usr/local/lib:${LD_LIBRARY_PATH}
ENV PATH=/usr/local/bin:${PATH}

# ============================================================
# 安装 OpenCV 和 PaddleOCR 及其依赖
# ============================================================
RUN pip3 install --no-cache-dir --upgrade pip setuptools wheel && \
    # 1. 安装 PaddlePaddle CPU 稳定版
    pip3 install --no-cache-dir paddlepaddle -i https://www.paddlepaddle.org.cn/packages/stable/cpu/ && \
    # 2. 安装无 GUI 版本的 OpenCV 以及 PaddleOCR
    pip3 install --no-cache-dir opencv-python-headless paddleocr==2.8.1


# 刷新字体缓存
RUN fc-cache -fv