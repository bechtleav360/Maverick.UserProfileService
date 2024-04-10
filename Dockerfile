FROM squidfunk/mkdocs-material

RUN pip install --upgrade pip

RUN pip install \
    pymdown-extensions \
    mkdocs \
    mkdocs-material \
    mkdocs-mermaid2-plugin \
  #  mkdocs-drawio-exporter \
    mkdocs-rtd-dropdown \
    mkdocs-git-revision-date-plugin \
    mkdocs-git-revision-date-localized-plugin \
    mkdocs-yamp \
    mkdocs-macros-plugin \
    mkdocs-print-site-plugin \
    mkdocs-awesome-pages-plugin